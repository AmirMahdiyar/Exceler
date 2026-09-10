using Exceler.Abstractions;
using Exceler.Configuration;
using Exceler.Core;
using Exceler.Core.Exceptions;
using Exceler.DependencyInjection;
using Exceler.Pipeline.Read;
using Exceler.Tests.Common.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Exceler.Tests.Unit.Readers
{
    public class ReaderEdgeCasesTests
    {
        private class DummyItem
        {
            public string Val { get; set; } = string.Empty;
        }

        private class DummyProfile : ExcelProfile<DummyItem>
        {
            public DummyProfile()
            {
                Map(x => x.Val).ToColumn(1).WithHeader("Header");
            }
        }

        private class EmptyProfile : ExcelProfile<DummyItem>
        {
            public EmptyProfile()
            {
            }
        }

        [Fact]
        public void Reading_nonexistent_sheet_fails_with_clear_error()
        {
            var sut = ExcelerTestBed.CreateReader<DummyItem, DummyProfile>();
            using var stream = ExcelStreamBuilder.EmptySheet("RealSheet");

            var act = () => sut.Read<DummyItem, DummyItem>(stream, "NonExistentSheet").ToList();

            act.Should().Throw<ArgumentException>()
                .WithMessage("*NonExistentSheet*");
        }

        [Fact]
        public void Reading_worksheet_without_dimension_yields_no_rows()
        {
            var sut = ExcelerTestBed.CreateReader<DummyItem, DummyProfile>();
            using var stream = ExcelStreamBuilder.EmptySheet("EmptySheet");

            var results = sut.Read<DummyItem, DummyItem>(stream, "EmptySheet").ToList();

            results.Should().BeEmpty();
        }

        [Fact]
        public void Reading_worksheet_with_unmapped_profile_yields_no_rows()
        {
            var sut = ExcelerTestBed.CreateReader<DummyItem, EmptyProfile>();
            using var stream = new ExcelStreamBuilder()
                .WithRow(1, "SomeData")
                .WithRow(2, "MoreData")
                .Build();

            var results = sut.Read<DummyItem, DummyItem>(stream).ToList();

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task Reading_empty_worksheet_in_chunks_yields_no_data()
        {
            var sut = ExcelerTestBed.CreateReader<DummyItem, DummyProfile>();
            using var stream = ExcelStreamBuilder.EmptySheet("EmptySheet");

            var chunks = new List<List<ExcelRowResult<DummyItem>>>();
            await foreach (var chunk in sut.ReadInChunksAsync<DummyItem, DummyItem>(stream, sheetName: "EmptySheet"))
            {
                chunks.Add(chunk);
            }

            chunks.Should().BeEmpty();
        }

        [Fact]
        public void Cast_exception_preserves_error_message_and_root_cause()
        {
            var inner = new FormatException("Bad format");
            var sutWithMessage = new ExcelCastException("Custom message");
            var sutWithInner = new ExcelCastException("Custom message", inner);

            sutWithMessage.Message.Should().Be("Custom message");
            sutWithInner.Message.Should().Be("Custom message");
            sutWithInner.InnerException.Should().BeSameAs(inner);
        }

        [Fact]
        public void Template_mismatch_exception_retains_unmatched_headers()
        {
            var errors = new List<string> { "Header 1 missing", "Header 2 mismatch" };
            var sut = new ExcelTemplateMismatchException(errors);
            var sutNull = new ExcelTemplateMismatchException(null!);

            sut.MissingOrInvalidHeaders.Should().BeEquivalentTo(errors);
            sutNull.MissingOrInvalidHeaders.Should().BeEmpty();
        }

        [Fact]
        public void Pass_through_processor_rejects_incompatible_input_and_output_types()
        {
            var sut = new PassThroughProcessor<string, int>();

            var act = () => sut.Process("TestString");

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Cannot pass through model of type*");
        }

        [Fact]
        public void Service_registration_applies_configuration_delegate()
        {
            var services = new ServiceCollection();
            bool actionExecuted = false;

            services.AddExcelCore(builder =>
            {
                actionExecuted = true;
                builder.UseNonCommercialLicense();
            });

            actionExecuted.Should().BeTrue();
            services.Should().Contain(d => d.ServiceType == typeof(IExcelReader));
            services.Should().Contain(d => d.ServiceType == typeof(IExcelWriter));
        }

        [Fact]
        public void Registration_without_license_fails_validation()
        {
            var services = new ServiceCollection();

            var act = () => services.AddExcelCore(_ => { });

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*You MUST explicitly accept the license terms*");
        }

        [Fact]
        public void Default_registration_without_license_fails_validation()
        {
            var services = new ServiceCollection();

            var act = () => services.AddExcelCore();

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*You MUST explicitly accept the license terms*");
        }

        private class TransformingReadHandler : ReadHandler<DummyItem, DummyItem>
        {
            public override void Handle(ReadContext<DummyItem, DummyItem> context)
            {
                context.Result.Data = new DummyItem { Val = "TransformedOutput" };
            }
        }

        [Fact]
        public async Task Asynchronous_execution_on_read_handler_mutates_row_context_state()
        {
            var sut = new TransformingReadHandler();
            var context = new ReadContext<DummyItem, DummyItem>(1);

            await sut.HandleAsync(context);

            context.Result.Data?.Val.Should().Be("TransformedOutput");
        }
    }
}

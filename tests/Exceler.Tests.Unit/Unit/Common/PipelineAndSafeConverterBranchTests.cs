using Exceler.Abstractions;
using Exceler.Configuration;
using Exceler.Core;
using Exceler.Core.Converter;
using Exceler.Core.EPPlus;
using Exceler.Pipeline.Read;
using Exceler.Pipeline.Read.Handlers;
using Exceler.Pipeline.Write;
using Exceler.Pipeline.Write.Handlers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Exceler.Tests.Unit.Unit.Common
{
    public class PipelineAndSafeConverterBranchTests
    {
        [Fact]
        public void SafeConverter_AlreadyTypedValues_CoverBranches()
        {
            var now = DateTime.UtcNow;
            SafeConverter.ChangeType<DateTime>(now).Should().Be(now);

            var dateOnly = new DateOnly(2025, 5, 20);
            SafeConverter.ChangeType<DateOnly>(dateOnly).Should().Be(dateOnly);
            SafeConverter.ChangeType<DateTime>(dateOnly).Should().Be(dateOnly.ToDateTime(TimeOnly.MinValue));

            var timeOnly = new TimeOnly(14, 30);
            SafeConverter.ChangeType<TimeOnly>(timeOnly).Should().Be(timeOnly);

            var timeSpan = TimeSpan.FromHours(3);
            SafeConverter.ChangeType<TimeSpan>(timeSpan).Should().Be(timeSpan);

            var guid = Guid.NewGuid();
            SafeConverter.ChangeType<Guid>(guid).Should().Be(guid);

            // Numeric cross conversions
            SafeConverter.ChangeType<decimal>((decimal)123.45).Should().Be(123.45m);
            SafeConverter.ChangeType<decimal>((int)123).Should().Be(123m);
            SafeConverter.ChangeType<decimal>((long)456L).Should().Be(456m);
            SafeConverter.ChangeType<decimal>((float)78.5f).Should().Be((decimal)78.5f);

            SafeConverter.ChangeType<double>((double)123.45).Should().Be(123.45);
            SafeConverter.ChangeType<double>((int)123).Should().Be(123.0);
            SafeConverter.ChangeType<double>((long)456L).Should().Be(456.0);
            SafeConverter.ChangeType<double>((decimal)78.5m).Should().Be(78.5);

            // Comma-separated numbers in German culture where comma is decimal separator
            var prevCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");
                SafeConverter.ChangeType<decimal>("123,45").Should().Be(123.45m);
                SafeConverter.ChangeType<double>("123,45").Should().Be(123.45);
            }
            finally
            {
                CultureInfo.CurrentCulture = prevCulture;
            }
        }

        private class BoxedModel
        {
            public int Number { get; set; }
        }

        private class BoxedModelProfile : ExcelProfile<BoxedModel>
        {
            public BoxedModelProfile()
            {
                // UnaryExpression boxing to cover MappingExpressionBuilder UnaryExpression operand branch
                Map(x => (object)x.Number).ToColumn(1);
            }
        }

        [Fact]
        public void MappingExpressionBuilder_UnaryExpression_CoverBranch()
        {
            var profile = new BoxedModelProfile();
            profile.EnsureBuilt();

            profile.CompiledSetters.Should().ContainKey(1);
            profile.CompiledGetters.Should().ContainKey(1);

            var model = new BoxedModel();
            profile.CompiledSetters[1](model, "42");
            model.Number.Should().Be(42);

            profile.CompiledGetters[1](model).Should().Be(42);
        }

        private class DummyModel
        {
            public string Val { get; set; } = string.Empty;
        }

        private class DummySyncValidator : IExcelValidator<DummyModel>
        {
            public IEnumerable<string> Validate(DummyModel model) => null!; // Returning null to test null validation errors branch
        }

        private class DummySyncProcessor : IExcelProcessor<DummyModel, DummyModel>
        {
            public DummyModel Process(DummyModel model) => model;
        }

        [Fact]
        public async Task ValidationAndProcessHandlers_SyncInAsyncAndNull_CoverBranches()
        {
            var context = new ReadContext<DummyModel, DummyModel>(1)
            {
                Validator = new DummySyncValidator(),
                Processor = new DummySyncProcessor()
            };
            context.InputModel.Val = "test";

            // 1. Test ValidateHandler HandleAsync with a synchronous validator
            var validateHandler = new ValidateHandler<DummyModel, DummyModel>();
            await validateHandler.HandleAsync(context, CancellationToken.None);
            context.Result.IsValid.Should().BeTrue();

            // 2. Test ProcessHandler HandleAsync with a synchronous processor
            var processHandler = new ProcessHandler<DummyModel, DummyModel>();
            await processHandler.HandleAsync(context, CancellationToken.None);
            context.Result.Data.Should().NotBeNull();
            context.Result.Data!.Val.Should().Be("test");
        }

        private class StyleTestModel
        {
            public string Text { get; set; } = "hello";
        }

        private class NoHeaderProfile : ExcelProfile<StyleTestModel>
        {
            public NoHeaderProfile()
            {
                AutoFitColumns = false; // Cover AutoFitColumns = false in FormattingWriterHandler
                Map(x => x.Text).ToColumn(1); // No header set -> startRow = 1 in StyleWriterHandler
            }
        }

        [Fact]
        public async Task FormattingAndStyleHandlers_Branches_Covered()
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("TestSheet");
            worksheet.Cells[1, 1].Value = "hello";

            var profile = new NoHeaderProfile();
            profile.EnsureBuilt();

            var context = new WriteContext<StyleTestModel>
            {
                Worksheet = worksheet,
                Profile = profile,
                Data = new[] { new StyleTestModel() }
            };

            var styleHandler = new StyleWriterHandler<StyleTestModel>();
            var formattingHandler = new FormattingWriterHandler<StyleTestModel>();
            styleHandler.SetNext(formattingHandler);

            await styleHandler.HandleAsync(context);
            worksheet.Cells[1, 1].Value.Should().Be("hello");
        }

        private class FullStyleModel
        {
            public decimal Price { get; set; } = 99.95m;
        }

        private class FullStyleProfile : ExcelProfile<FullStyleModel>
        {
            public FullStyleProfile()
            {
                Map(x => x.Price)
                    .ToColumn(1)
                    .WithHeader("Price Header")
                    .WithNumberFormat("$#,##0.00")
                    .WithBackgroundColor("#FF0000")
                    .WithFontColor("#FFFFFF")
                    .IsBold();
            }
        }

        [Fact]
        public async Task EPPlusWriterEngine_CustomSheetName_AndFullStyling()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ExcelProfile<FullStyleModel>>(new FullStyleProfile());
            var provider = services.BuildServiceProvider();

            var writer = new EPPlusWriterEngine(provider);
            var bytes = await writer.Write(new[] { new FullStyleModel() }, "MyCustomSheet");

            using var ms = new MemoryStream(bytes);
            using var package = new ExcelPackage(ms);
            package.Workbook.Worksheets.Count.Should().Be(1);
            package.Workbook.Worksheets[0].Name.Should().Be("MyCustomSheet");
            package.Workbook.Worksheets[0].Cells[2, 1].Style.Font.Bold.Should().BeTrue();
        }

        private class TrimTestModel
        {
            public string? Text { get; set; }
        }

        private class TrimTestProfile : ExcelProfile<TrimTestModel>
        {
            public TrimTestProfile()
            {
                TrimStringValues = true;
                Map(x => x.Text).ToColumn(1); // No header to test GetColumnName fallback to column number
            }
        }

        [Fact]
        public async Task ParseHandler_TrimStringValues_WhitespaceBecomesNull()
        {
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Sheet1");
            ws.Cells[1, 1].Value = "   "; // Whitespace only

            var profile = new TrimTestProfile();
            profile.EnsureBuilt();

            var context = new ReadContext<TrimTestModel, TrimTestModel>(1)
            {
                Worksheet = ws,
                ColCount = 1,
                Profile = profile
            };

            var parseHandler = new ParseHandler<TrimTestModel, TrimTestModel>();
            await parseHandler.HandleAsync(context);

            context.InputModel.Text.Should().BeNull();
        }
    }
}

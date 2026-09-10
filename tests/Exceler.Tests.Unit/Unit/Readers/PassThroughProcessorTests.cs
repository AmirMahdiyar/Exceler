using Exceler.Abstractions;
using Exceler.Configuration;
using Exceler.Core;
using Exceler.Tests.Infrastructure;
using Exceler.Tests.Infrastructure.Base;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exceler.Tests.Unit.Readers
{
    public class SimpleReportModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class SimpleReportProfile : ExcelProfile<SimpleReportModel>
    {
        public SimpleReportProfile()
        {
            Map(x => x.Id).ToColumn(1).WithHeader("Report ID");
            Map(x => x.Title).ToColumn(2).WithHeader("Report Title");
            Map(x => x.Amount).ToColumn(3).WithHeader("Report Amount");
        }
    }

    public class DifferentOutputModel
    {
        public string DisplayText { get; set; } = string.Empty;
    }

    public class CustomProcessedModel
    {
        public int Id { get; set; }
        public string Note { get; set; } = string.Empty;
    }

    public class CustomProcessedProfile : ExcelProfile<CustomProcessedModel>
    {
        public CustomProcessedProfile()
        {
            Map(x => x.Id).ToColumn(1).WithHeader("ID");
            Map(x => x.Note).ToColumn(2).WithHeader("Note");
        }
    }

    public class CustomNoteProcessor : IExcelProcessor<CustomProcessedModel, CustomProcessedModel>
    {
        public CustomProcessedModel Process(CustomProcessedModel input)
        {
            return new CustomProcessedModel
            {
                Id = input.Id,
                Note = $"Processed: {input.Note}"
            };
        }
    }

    public class PassThroughProcessorTests : ExcelerTestBase
    {
        [Fact]
        public void WhenNoProcessorIsRegistered_IdenticalInputAndOutputTypesUseDefaultPassThroughProcessor()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("PassThroughDirect");
            using var stream = builder
                .WithHeaders("Report ID", "Report Title", "Report Amount")
                .WithRow(2, 101, "Quarterly Summary", 4500.50m)
                .Build();

            // Act
            var results = Reader.Read<SimpleReportModel, SimpleReportModel>(stream).ToList();

            // Assert
            results.Should().ContainSingle();
            var result = results.First();

            result.IsValid.Should().BeTrue();
            result.Data!.Id.Should().Be(101);
            result.Data.Title.Should().Be("Quarterly Summary");
            result.Data.Amount.Should().Be(4500.50m);
        }

        [Fact]
        public async Task WhenNoProcessorIsRegisteredInChunkedRead_DefaultPassThroughProcessorWorksSeamlessly()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("PassThroughChunked");
            using var stream = builder
                .WithHeaders("Report ID", "Report Title", "Report Amount")
                .WithRow(2, 201, "Audit Log", 1200m)
                .WithRow(3, 202, "Tax Summary", 3400m)
                .Build();

            // Act
            var allRows = new List<ExcelRowResult<SimpleReportModel>>();
            await foreach (var chunk in Reader.ReadInChunksAsync<SimpleReportModel, SimpleReportModel>(stream, chunkSize: 5))
            {
                allRows.AddRange(chunk);
            }

            // Assert
            allRows.Should().HaveCount(2);
            allRows[0].Data!.Title.Should().Be("Audit Log");
            allRows[1].Data!.Title.Should().Be("Tax Summary");
        }

        [Fact]
        public void WhenCustomProcessorIsRegistered_CustomProcessorTakesPrecedenceOverPassThrough()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("CustomPrecedence");
            using var stream = builder
                .WithHeaders("ID", "Note")
                .WithRow(2, 1, "Urgent Task")
                .Build();

            // Act
            var results = Reader.Read<CustomProcessedModel, CustomProcessedModel>(stream).ToList();

            // Assert
            results.Should().ContainSingle();
            results[0].Data!.Note.Should().Be("Processed: Urgent Task");
        }

        [Fact]
        public void WhenNoProcessorIsRegisteredAndTypesDiffer_ThrowsInvalidOperationExceptionWithHelpfulMessage()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("DifferingTypesNoProcessor");
            using var stream = builder
                .WithHeaders("Report ID", "Report Title", "Report Amount")
                .WithRow(2, 1, "Some Title", 100m)
                .Build();

            // Act
            Action act = () =>
            {
                var _ = Reader.Read<SimpleReportModel, DifferentOutputModel>(stream).ToList();
            };

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*No processor registered for converting*");
        }
    }
}

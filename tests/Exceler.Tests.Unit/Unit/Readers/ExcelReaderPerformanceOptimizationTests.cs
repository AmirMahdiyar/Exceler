using Exceler.Configuration;
using Exceler.Tests.Common.Fixtures;
using Exceler.Tests.Common.Fixtures;
using FluentAssertions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Exceler.Tests.Unit.Readers
{
    public class PerformanceOptimizedModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
    }

    public class PerformanceOptimizedProfile : ExcelProfile<PerformanceOptimizedModel>
    {
        public PerformanceOptimizedProfile()
        {
            Map(x => x.Id).ToColumn(1).WithHeader("ID");
            Map(x => x.Title).ToColumn(2).WithHeader("Title");
            Map(x => x.Quantity).ToColumn(3).WithHeader("Quantity");
        }
    }

    public class ExcelReaderPerformanceOptimizationTests : ExcelerTestBase
    {
        [Fact]
        public void WhenReadingWorksheetWithTrailingAndIntermittentEmptyRows_EmptyRowsAreSkippedAccurately()
        {
            // Arrange: Rows 2 and 4 are valid, Row 3 is empty, Rows 5-7 are empty
            using var builder = new ExcelStreamBuilder("Data");
            using var stream = builder
                .WithHeaders("ID", "Title", "Quantity")
                .WithRow(2, 101, "First Item", 10.5m)
                .WithRow(4, 102, "Second Item", 25.0m)
                .Build();

            // Act
            var results = Reader.Read<PerformanceOptimizedModel, PerformanceOptimizedModel>(stream).ToList();

            // Assert
            results.Should().HaveCount(2);
            results[0].Data!.Id.Should().Be(101);
            results[0].Data!.Title.Should().Be("First Item");
            results[1].Data!.Id.Should().Be(102);
            results[1].Data!.Title.Should().Be("Second Item");
        }

        [Fact]
        public void WhenReadingWorksheetWithWhitespaceOnlyRows_RowsAreSkippedAsEmpty()
        {
            // Arrange: Row 2 has only spaces in text cells
            using var builder = new ExcelStreamBuilder("Data");
            using var stream = builder
                .WithHeaders("ID", "Title", "Quantity")
                .WithRow(2, "   ", "   ", "   ")
                .WithRow(3, 201, "Real Item", 50.0m)
                .Build();

            // Act
            var results = Reader.Read<PerformanceOptimizedModel, PerformanceOptimizedModel>(stream).ToList();

            // Assert
            results.Should().HaveCount(1);
            results[0].Data!.Id.Should().Be(201);
            results[0].Data!.Title.Should().Be("Real Item");
        }

        [Fact]
        public void WhenReadingRowWithZeroNumericValues_RowIsNotConsideredEmpty()
        {
            // Arrange: ID = 0, Title = "Zero", Quantity = 0.0m (0 is valid data, not empty)
            using var builder = new ExcelStreamBuilder("Data");
            using var stream = builder
                .WithHeaders("ID", "Title", "Quantity")
                .WithRow(2, 0, "Zero", 0.0m)
                .Build();

            // Act
            var results = Reader.Read<PerformanceOptimizedModel, PerformanceOptimizedModel>(stream).ToList();

            // Assert
            results.Should().ContainSingle();
            results[0].IsValid.Should().BeTrue();
            results[0].Data!.Id.Should().Be(0);
            results[0].Data!.Quantity.Should().Be(0.0m);
        }

        [Fact]
        public async Task WhenReadingInChunksAsyncWithEmptyRows_ChunksExcludeEmptyRowsProperly()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("Data");
            using var stream = builder
                .WithHeaders("ID", "Title", "Quantity")
                .WithRow(2, 301, "Item A", 1.0m)
                .WithRow(5, 302, "Item B", 2.0m)
                .Build();

            // Act
            var chunks = new List<PerformanceOptimizedModel>();
            await foreach (var chunk in Reader.ReadInChunksAsync<PerformanceOptimizedModel, PerformanceOptimizedModel>(stream, chunkSize: 10))
            {
                chunks.AddRange(chunk.Select(c => c.Data!));
            }

            // Assert
            chunks.Should().HaveCount(2);
            chunks[0].Id.Should().Be(301);
            chunks[1].Id.Should().Be(302);
        }
    }
}

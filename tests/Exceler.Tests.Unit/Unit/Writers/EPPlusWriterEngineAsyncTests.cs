using Exceler.Abstractions;
using Exceler.Configuration;
using Exceler.Core.EPPlus;
using Exceler.Extensions;
using Exceler.Tests.Common.Fixtures;
using FluentAssertions;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Exceler.Tests.Unit.Writers
{
    public class EPPlusAsyncItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateOnly DateVal { get; set; }
        public TimeOnly TimeVal { get; set; }
    }

    public class EPPlusAsyncProfile : ExcelProfile<EPPlusAsyncItem>
    {
        public EPPlusAsyncProfile()
        {
            Map(x => x.Id).ToColumn(1).WithHeader("ID");
            Map(x => x.Title).ToColumn(2).WithHeader("Title");
            Map(x => x.DateVal).ToColumn(3).WithHeader("Date");
            Map(x => x.TimeVal).ToColumn(4).WithHeader("Time");
        }
    }

    public class EPPlusWriterEngineAsyncTests : ExcelerTestBase
    {
        [Fact]
        public async Task EPPlus_WriteAsync_with_IEnumerable_writes_directly_to_stream()
        {
            // Arrange
            var writer = ExcelerTestBed.CreateWriter<EPPlusAsyncItem, EPPlusAsyncProfile>(ExcelerEngine.EPPlus);
            var items = new List<EPPlusAsyncItem>
            {
                new() { Id = 1, Title = "Test 1", DateVal = new DateOnly(2026, 5, 1), TimeVal = new TimeOnly(12, 0, 0) },
                new() { Id = 2, Title = "Test 2", DateVal = new DateOnly(2026, 5, 2), TimeVal = new TimeOnly(13, 30, 0) }
            };

            using var stream = new MemoryStream();

            // Act
            await writer.WriteAsync(items, stream, "EPPlusSyncStream");
            stream.Position = 0;

            // Assert
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets["EPPlusSyncStream"];
            sheet.Should().NotBeNull();
            sheet.Dimension.Rows.Should().Be(3); // Header + 2 rows
            sheet.Cells[2, 2].Text.Should().Be("Test 1");
            sheet.Cells[3, 2].Text.Should().Be("Test 2");
        }

        [Fact]
        public async Task EPPlus_WriteAsync_with_IAsyncEnumerable_streams_and_formats_dates_and_times()
        {
            // Arrange
            var writer = ExcelerTestBed.CreateWriter<EPPlusAsyncItem, EPPlusAsyncProfile>(ExcelerEngine.EPPlus);

            async IAsyncEnumerable<EPPlusAsyncItem> GenerateAsyncStream()
            {
                yield return null!; // Tests null item skip
                yield return new EPPlusAsyncItem
                {
                    Id = 10,
                    Title = "Async Item",
                    DateVal = new DateOnly(2026, 8, 20),
                    TimeVal = new TimeOnly(9, 15, 0)
                };
                await Task.Yield();
            }

            using var stream = new MemoryStream();

            // Act
            await writer.WriteAsync(GenerateAsyncStream(), stream, "EPPlusAsyncStream");
            stream.Position = 0;

            // Assert
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets["EPPlusAsyncStream"];
            sheet.Should().NotBeNull();
            sheet.Dimension.Rows.Should().Be(2); // Header + 1 non-null item
            sheet.Cells[2, 1].Value.Should().Be(10);
            sheet.Cells[2, 2].Text.Should().Be("Async Item");
            sheet.Cells[2, 3].Style.Numberformat.Format.Should().Be("yyyy-mm-dd");
            sheet.Cells[2, 4].Style.Numberformat.Format.Should().Be("hh:mm:ss");
        }

        [Fact]
        public async Task EPPlus_ToExcelAsync_extension_streams_successfully()
        {
            // Arrange
            var writer = ExcelerTestBed.CreateWriter<EPPlusAsyncItem, EPPlusAsyncProfile>(ExcelerEngine.EPPlus);

            async IAsyncEnumerable<EPPlusAsyncItem> Stream()
            {
                yield return new EPPlusAsyncItem { Id = 1, Title = "Extension Item" };
                await Task.Yield();
            }

            using var stream = new MemoryStream();

            // Act
            await Stream().ToExcelAsync(writer, stream, "ExtensionSheet");
            stream.Position = 0;

            // Assert
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets["ExtensionSheet"];
            sheet.Should().NotBeNull();
            sheet.Cells[2, 2].Text.Should().Be("Extension Item");
        }
    }
}

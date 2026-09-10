using Exceler.Abstractions;
using Exceler.Configuration;
using Exceler.Extensions;

using FluentAssertions;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Exceler.Tests.Unit.Extensions
{
    public class ExcelerExtensionsTests
    {
        private class ExtensionTestItem
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
        }

        private class ExtensionTestProfile : ExcelProfile<ExtensionTestItem>
        {
            public ExtensionTestProfile()
            {
                Map(x => x.Id).ToColumn(1).WithHeader("ID");
                Map(x => x.Title).ToColumn(2).WithHeader("Title");
            }
        }

        private readonly IExcelWriter _writer = ExcelerTestBed.CreateWriter<ExtensionTestItem, ExtensionTestProfile>();

        [Fact]
        public async Task Asynchronous_data_stream_exports_directly_to_target_stream()
        {
            async IAsyncEnumerable<ExtensionTestItem> GetAsyncItems()
            {
                yield return new ExtensionTestItem { Id = 10, Title = "Extension Alpha" };
                await Task.Yield();
                yield return new ExtensionTestItem { Id = 20, Title = "Extension Beta" };
            }

            var sut = GetAsyncItems();
            using var stream = new MemoryStream();

            await sut.ToExcelAsync(_writer, stream, "CustomAsyncSheet");

            stream.Length.Should().BeGreaterThan(0);
            stream.Position = 0;
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets["CustomAsyncSheet"];
            sheet.Should().NotBeNull();
            sheet.Cells[2, 1].Value.Should().Be(10);
            sheet.Cells[2, 2].Value.Should().Be("Extension Alpha");
            sheet.Cells[3, 1].Value.Should().Be(20);
            sheet.Cells[3, 2].Value.Should().Be("Extension Beta");
        }

        [Fact]
        public async Task Synchronous_collection_exports_directly_to_target_stream()
        {
            var sut = new List<ExtensionTestItem>
            {
                new() { Id = 99, Title = "Stream Test" }
            };
            using var stream = new MemoryStream();

            await sut.ToExcelAsync(_writer, stream, "CustomSyncSheet");

            stream.Length.Should().BeGreaterThan(0);
            stream.Position = 0;
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets["CustomSyncSheet"];
            sheet.Should().NotBeNull();
            sheet.Cells[2, 1].Value.Should().Be(99);
            sheet.Cells[2, 2].Value.Should().Be("Stream Test");
        }
    }
}

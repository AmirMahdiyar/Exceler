using Exceler.Configuration;
using Exceler.Extensions;
using Exceler.Tests.Infrastructure.Base;
using FluentAssertions;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Exceler.Tests.Unit.Writers
{
    public record PositionalProductRecord(int Id, string Title, decimal Price);

    public class PositionalProductRecordProfile : ExcelProfile<PositionalProductRecord>
    {
        public PositionalProductRecordProfile()
        {
            Map(x => x.Id).ToColumn(1).WithHeader("Product ID");
            Map(x => x.Title).ToColumn(2).WithHeader("Title");
            Map(x => x.Price).ToColumn(3).WithHeader("Price");
        }
    }

    public class ImmutablePersonClass
    {
        public ImmutablePersonClass(int ssn, string fullName)
        {
            Ssn = ssn;
            FullName = fullName;
        }

        public int Ssn { get; }
        public string FullName { get; }
    }

    public class ImmutablePersonClassProfile : ExcelProfile<ImmutablePersonClass>
    {
        public ImmutablePersonClassProfile()
        {
            Map(x => x.Ssn).ToColumn(1).WithHeader("SSN");
            Map(x => x.FullName).ToColumn(2).WithHeader("Full Name");
        }
    }

    public class ExcelWriterModelConstraintsTests : ExcelerTestBase
    {
        [Fact]
        public async Task WhenWritingPositionalRecords_GeneratesValidExcelWithExpectedContent()
        {
            // Arrange
            var items = new List<PositionalProductRecord>
            {
                new(101, "Mechanical Keyboard", 120.50m),
                new(102, "Wireless Mouse", 49.99m)
            };

            // Act
            var bytes = await Writer.Write(items);

            // Assert
            using var stream = new MemoryStream(bytes);
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            sheet.Cells[1, 1].Text.Should().Be("Product ID");
            sheet.Cells[1, 2].Text.Should().Be("Title");
            sheet.Cells[1, 3].Text.Should().Be("Price");

            sheet.Cells[2, 1].Value.Should().Be(101);
            sheet.Cells[2, 2].Text.Should().Be("Mechanical Keyboard");
            sheet.Cells[2, 3].Value.Should().Be(120.50m);

            sheet.Cells[3, 1].Value.Should().Be(102);
            sheet.Cells[3, 2].Text.Should().Be("Wireless Mouse");
            sheet.Cells[3, 3].Value.Should().Be(49.99m);
        }

        [Fact]
        public async Task WhenWritingImmutableClassesWithoutParameterlessConstructor_GeneratesValidExcelWithExpectedContent()
        {
            // Arrange
            var people = new List<ImmutablePersonClass>
            {
                new(1001, "Alice Cooper"),
                new(1002, "Bob Dylan")
            };

            // Act
            var bytes = await Writer.Write(people);

            // Assert
            using var stream = new MemoryStream(bytes);
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            sheet.Cells[1, 1].Text.Should().Be("SSN");
            sheet.Cells[1, 2].Text.Should().Be("Full Name");

            sheet.Cells[2, 1].Value.Should().Be(1001);
            sheet.Cells[2, 2].Text.Should().Be("Alice Cooper");

            sheet.Cells[3, 1].Value.Should().Be(1002);
            sheet.Cells[3, 2].Text.Should().Be("Bob Dylan");
        }

        [Fact]
        public async Task WhenWritingPositionalRecordsToStreamAsync_WritesCorrectDataToStream()
        {
            // Arrange
            var items = new List<PositionalProductRecord>
            {
                new(201, "Monitor 4K", 450.00m)
            };
            using var outputStream = new MemoryStream();

            // Act
            await Writer.WriteAsync(items, outputStream);

            // Assert
            outputStream.Position = 0;
            using var package = new ExcelPackage(outputStream);
            var sheet = package.Workbook.Worksheets[0];

            sheet.Cells[2, 1].Value.Should().Be(201);
            sheet.Cells[2, 2].Text.Should().Be("Monitor 4K");
            sheet.Cells[2, 3].Value.Should().Be(450.00m);
        }

        [Fact]
        public async Task WhenWritingAsyncEnumerableOfPositionalRecords_WritesCorrectDataToStream()
        {
            // Arrange
            async IAsyncEnumerable<PositionalProductRecord> GenerateItemsAsync()
            {
                yield return new PositionalProductRecord(301, "Desk Mat", 25.00m);
                await Task.Yield();
                yield return new PositionalProductRecord(302, "USB Hub", 35.00m);
            }

            using var outputStream = new MemoryStream();

            // Act
            await Writer.WriteAsync(GenerateItemsAsync(), outputStream);

            // Assert
            outputStream.Position = 0;
            using var package = new ExcelPackage(outputStream);
            var sheet = package.Workbook.Worksheets[0];

            sheet.Cells[2, 1].Value.Should().Be(301);
            sheet.Cells[2, 2].Text.Should().Be("Desk Mat");
            sheet.Cells[3, 1].Value.Should().Be(302);
            sheet.Cells[3, 2].Text.Should().Be("USB Hub");
        }

        [Fact]
        public async Task WhenWritingViaToExcelAsyncExtension_WritesCorrectDataToStream()
        {
            // Arrange
            var items = new List<PositionalProductRecord>
            {
                new(401, "Webcam", 79.99m)
            };
            using var outputStream = new MemoryStream();

            // Act
            await items.ToExcelAsync(Writer, outputStream);

            // Assert
            outputStream.Position = 0;
            using var package = new ExcelPackage(outputStream);
            var sheet = package.Workbook.Worksheets[0];

            sheet.Cells[2, 1].Value.Should().Be(401);
            sheet.Cells[2, 2].Text.Should().Be("Webcam");
            sheet.Cells[2, 3].Value.Should().Be(79.99m);
        }
    }
}

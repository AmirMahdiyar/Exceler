using Exceler.Abstractions;
using Exceler.Configuration;
using Exceler.Core.OpenXml;
using Exceler.Tests.Common.Fixtures;
using Exceler.Tests.Common.TestDoubles.Models;
using FluentAssertions;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Exceler.Tests.Unit.Writers
{
    public class OpenXmlWriterEngineTests : ExcelerTestBase
    {
        #region Test Models & Profiles

        public class OpenXmlTestItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public bool IsActive { get; set; }
            public DateOnly StartDate { get; set; }
            public TimeOnly ShiftTime { get; set; }
            public int? OptionalCount { get; set; }
        }

        public class OpenXmlTestProfile : ExcelProfile<OpenXmlTestItem>
        {
            public OpenXmlTestProfile()
            {
                Map(x => x.Id).ToColumn(1).WithHeader("Item ID").WithWidth(15.0).IsBold();
                Map(x => x.Name).ToColumn(2).WithHeader("Item Name").WithWidth(30.0);
                Map(x => x.Amount).ToColumn(3).WithHeader("Amount").WithNumberFormat("$#,##0.00");
                Map(x => x.IsActive).ToColumn(4).WithHeader("Active");
                Map(x => x.StartDate).ToColumn(5).WithHeader("Start Date");
                Map(x => x.ShiftTime).ToColumn(6).WithHeader("Shift Time");
                Map(x => x.OptionalCount).ToColumn(7).WithHeader("Optional");
            }
        }

        public class OpenXmlStyledItem
        {
            public string Code { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }

        public class OpenXmlStyledProfile : ExcelProfile<OpenXmlStyledItem>
        {
            public OpenXmlStyledProfile()
            {
                WithRightToLeft();
                WithAutoFitColumns(); // Should be a safe no-op in OpenXML mode

                Map(x => x.Code)
                    .ToColumn(1)
                    .WithHeader("Code")
                    .WithFontColor(ExcelColor.DarkBlue)
                    .WithBackgroundColor(ExcelColor.SoftYellow);

                Map(x => x.Description)
                    .ToColumn(2)
                    .WithHeader("Description")
                    .WithBackgroundColor("#FF0000");
            }
        }

        #endregion

        [Fact]
        public async Task OpenXml_exports_headers_and_data_accurately()
        {
            // Arrange
            var writer = ExcelerTestBed.CreateWriter<OpenXmlTestItem, OpenXmlTestProfile>(ExcelerEngine.OpenXml);
            var items = new List<OpenXmlTestItem>
            {
                new()
                {
                    Id = 101,
                    Name = "Enterprise Server",
                    Amount = 1499.99m,
                    IsActive = true,
                    StartDate = new DateOnly(2026, 3, 15),
                    ShiftTime = new TimeOnly(8, 30, 0),
                    OptionalCount = 42
                },
                new()
                {
                    Id = 102,
                    Name = "Network Switch",
                    Amount = 299.50m,
                    IsActive = false,
                    StartDate = new DateOnly(2026, 4, 1),
                    ShiftTime = new TimeOnly(17, 0, 0),
                    OptionalCount = null
                }
            };

            // Act
            byte[] bytes = await writer.Write(items, "Inventory");

            // Assert
            using var stream = new MemoryStream(bytes);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets["Inventory"];
            worksheet.Should().NotBeNull();

            // Verify Headers
            worksheet.Cells[1, 1].Text.Should().Be("Item ID");
            worksheet.Cells[1, 2].Text.Should().Be("Item Name");
            worksheet.Cells[1, 3].Text.Should().Be("Amount");
            worksheet.Cells[1, 4].Text.Should().Be("Active");
            worksheet.Cells[1, 5].Text.Should().Be("Start Date");
            worksheet.Cells[1, 6].Text.Should().Be("Shift Time");
            worksheet.Cells[1, 7].Text.Should().Be("Optional");

            // Verify Row 2
            worksheet.Cells[2, 1].Value.Should().Be(101.0);
            worksheet.Cells[2, 2].Text.Should().Be("Enterprise Server");
            Convert.ToDecimal(worksheet.Cells[2, 3].Value).Should().Be(1499.99m);
            worksheet.Cells[2, 4].Value.Should().Be(true);
            worksheet.Cells[2, 7].Value.Should().Be(42.0);

            // Verify Row 3
            worksheet.Cells[3, 1].Value.Should().Be(102.0);
            worksheet.Cells[3, 2].Text.Should().Be("Network Switch");
            Convert.ToDecimal(worksheet.Cells[3, 3].Value).Should().Be(299.50m);
            worksheet.Cells[3, 4].Value.Should().Be(false);
            worksheet.Cells[3, 7].Value.Should().BeNull();
        }

        [Fact]
        public async Task OpenXml_streams_large_async_enumerable_efficiently()
        {
            // Arrange
            var writer = ExcelerTestBed.CreateWriter<TestModel, TestModelProfile>(ExcelerEngine.OpenXml);
            const int recordCount = 3000;

            async IAsyncEnumerable<TestModel> StreamData()
            {
                for (int i = 1; i <= recordCount; i++)
                {
                    yield return new TestModel
                    {
                        Id = i,
                        FullName = $"Customer {i}",
                        Balance = i * 10.5m,
                        CreatedAt = new DateTime(2026, 1, 1)
                    };
                    if (i % 500 == 0) await Task.Yield();
                }
            }

            using var outputStream = new MemoryStream();

            // Act
            await writer.WriteAsync(StreamData(), outputStream, "StreamSheet");
            outputStream.Position = 0;

            // Assert
            using var package = new ExcelPackage(outputStream);
            var sheet = package.Workbook.Worksheets["StreamSheet"];
            sheet.Should().NotBeNull();
            sheet.Dimension.Rows.Should().Be(recordCount + 1); // Header + data
            sheet.Cells[2, 1].Value.Should().Be(1.0);
            sheet.Cells[2, 2].Text.Should().Be("Customer 1");
            sheet.Cells[recordCount + 1, 1].Value.Should().Be((double)recordCount);
        }

        [Fact]
        public async Task OpenXml_applies_explicit_column_widths()
        {
            // Arrange
            var writer = ExcelerTestBed.CreateWriter<OpenXmlTestItem, OpenXmlTestProfile>(ExcelerEngine.OpenXml);
            var items = new List<OpenXmlTestItem>
            {
                new() { Id = 1, Name = "Item A" }
            };

            // Act
            byte[] bytes = await writer.Write(items);

            // Assert
            using var stream = new MemoryStream(bytes);
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            sheet.Column(1).Width.Should().Be(15.0);
            sheet.Column(2).Width.Should().Be(30.0);
        }

        [Fact]
        public async Task OpenXml_sets_right_to_left_view()
        {
            // Arrange
            var writer = ExcelerTestBed.CreateWriter<OpenXmlStyledItem, OpenXmlStyledProfile>(ExcelerEngine.OpenXml);
            var items = new List<OpenXmlStyledItem>
            {
                new() { Code = "X1", Description = "RTL Description" }
            };

            // Act
            byte[] bytes = await writer.Write(items);

            // Assert
            using var stream = new MemoryStream(bytes);
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            sheet.View.RightToLeft.Should().BeTrue();
        }

        [Fact]
        public async Task OpenXml_gracefully_handles_autofit_columns_as_safe_noop()
        {
            // Arrange
            var writer = ExcelerTestBed.CreateWriter<OpenXmlStyledItem, OpenXmlStyledProfile>(ExcelerEngine.OpenXml);
            var items = new List<OpenXmlStyledItem>
            {
                new() { Code = "AUTO-01", Description = "Testing safe noop for AutoFitColumns" }
            };

            // Act & Assert (Should not throw and should output valid bytes)
            var act = async () => await writer.Write(items);
            var bytes = await act.Should().NotThrowAsync();
            bytes.Subject.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task OpenXml_sanitizes_sheet_name_with_invalid_characters_and_length()
        {
            // Arrange
            var writer = ExcelerTestBed.CreateWriter<TestModel, TestModelProfile>(ExcelerEngine.OpenXml);
            var items = new List<TestModel>
            {
                new() { Id = 1, FullName = "Name" }
            };

            // Sheet name contains invalid characters ':', '/', '?', '*', '[', ']' and exceeds 31 characters
            string rawSheetName = "Reports:2026/Q1?[Final]*A_Very_Long_Sheet_Name_Exceeding_Limit";

            // Act
            byte[] bytes = await writer.Write(items, rawSheetName);

            // Assert
            using var stream = new MemoryStream(bytes);
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            sheet.Name.Length.Should().BeLessThanOrEqualTo(31);
            sheet.Name.Should().NotContain(":");
            sheet.Name.Should().NotContain("/");
            sheet.Name.Should().NotContain("?");
            sheet.Name.Should().NotContain("*");
            sheet.Name.Should().NotContain("[");
            sheet.Name.Should().NotContain("]");
        }

        [Fact]
        public async Task Dual_engine_parity_both_engines_produce_equivalent_results()
        {
            // Arrange
            var openXmlWriter = ExcelerTestBed.CreateWriter<TestModel, TestModelProfile>(ExcelerEngine.OpenXml);
            var epPlusWriter = ExcelerTestBed.CreateWriter<TestModel, TestModelProfile>(ExcelerEngine.EPPlus);

            var sourceData = new List<TestModel>
            {
                new() { Id = 1, FullName = "Alice", Balance = 500.25m, CreatedAt = new DateTime(2026, 1, 10) },
                new() { Id = 2, FullName = "Bob", Balance = 750.50m, CreatedAt = new DateTime(2026, 2, 20) }
            };

            // Act
            byte[] openXmlBytes = await openXmlWriter.Write(sourceData, "Parity");
            byte[] epPlusBytes = await epPlusWriter.Write(sourceData, "Parity");

            // Assert: Read both back using IExcelReader
            using var openXmlStream = new MemoryStream(openXmlBytes);
            using var epPlusStream = new MemoryStream(epPlusBytes);

            var openXmlResults = Reader.Read<TestModel, TestModel>(openXmlStream, "Parity").ToList();
            var epPlusResults = Reader.Read<TestModel, TestModel>(epPlusStream, "Parity").ToList();

            openXmlResults.Should().HaveCount(2);
            epPlusResults.Should().HaveCount(2);

            openXmlResults[0].Data!.Id.Should().Be(epPlusResults[0].Data!.Id);
            openXmlResults[0].Data!.FullName.Should().Be(epPlusResults[0].Data!.FullName);
            openXmlResults[0].Data!.Balance.Should().Be(epPlusResults[0].Data!.Balance);

            openXmlResults[1].Data!.Id.Should().Be(epPlusResults[1].Data!.Id);
            openXmlResults[1].Data!.FullName.Should().Be(epPlusResults[1].Data!.FullName);
            openXmlResults[1].Data!.Balance.Should().Be(epPlusResults[1].Data!.Balance);
        }
    }
}

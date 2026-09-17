using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Exceler.Abstractions;
using Exceler.Configuration;
using Exceler.Core;
using Exceler.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Exceler.Tests.Unit.Writers
{
    public class ConditionalStyleIntegrationTests
    {
        public class TransactionModel
        {
            public int Id { get; set; }
            public string Description { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public string Status { get; set; } = string.Empty;
            public bool IsFlagged { get; set; }
        }

        public class TransactionProfile : ExcelProfile<TransactionModel>
        {
            public TransactionProfile()
            {
                // Row-level rule: Flagged transactions get a soft yellow background across all columns
                WithConditionalRowStyle(
                    x => x.IsFlagged,
                    s => s.WithBackgroundColor(ExcelColor.SoftYellow)
                );

                Map(x => x.Id)
                    .ToColumn(1)
                    .WithHeader("Transaction ID");

                Map(x => x.Description)
                    .ToColumn(2)
                    .WithHeader("Description")
                    .WithWidth(25);

                Map(x => x.Amount)
                    .ToColumn(3)
                    .WithHeader("Amount")
                    .WithFormat("$#,##0.00")
                    .WithConditionalStyle(
                        val => val < 0,
                        s => s.WithBackgroundColor(ExcelColor.SoftRed).WithFontColor(ExcelColor.DarkRed).SetBold(true)
                    )
                    .WithConditionalStyle(
                        val => val > 500,
                        s => s.WithBackgroundColor(ExcelColor.SoftGreen).WithFontColor(ExcelColor.DarkGreen)
                    );

                Map(x => x.Status)
                    .ToColumn(4)
                    .WithHeader("Status")
                    .WithConditionalStyle(
                        x => x.Status == "Rejected",
                        s => s.WithBackgroundColor(ExcelColor.SoftRed)
                    );
            }
        }

        private static List<TransactionModel> CreateSampleData()
        {
            return new List<TransactionModel>
            {
                // Row 2: Negative amount -> Amount col should have SoftRed background, bold, DarkRed font
                new TransactionModel { Id = 1, Description = "Refund", Amount = -75.50m, Status = "Completed", IsFlagged = false },
                // Row 3: Positive high amount -> Amount col should have SoftGreen background, DarkGreen font
                new TransactionModel { Id = 2, Description = "Client Payment", Amount = 1200.00m, Status = "Completed", IsFlagged = false },
                // Row 4: Flagged row -> Entire row SoftYellow, Status is "Rejected" so Status col overrides to SoftRed
                new TransactionModel { Id = 3, Description = "Suspicious Transfer", Amount = 100.00m, Status = "Rejected", IsFlagged = true },
                // Row 5: Normal row -> Base styles
                new TransactionModel { Id = 4, Description = "Normal Expense", Amount = 50.00m, Status = "Pending", IsFlagged = false }
            };
        }

        [Fact]
        public async Task OpenXml_Should_Emit_Correct_Conditional_Styles_For_Cells_And_Rows()
        {
            var services = new ServiceCollection();
            services.AddExcelCore(options =>
            {
                options.UseOpenXmlEngine();
                options.RegisterFromAssemblyContaining<TransactionProfile>();
            });
            var provider = services.BuildServiceProvider();
            var writer = provider.GetRequiredService<IExcelWriter>();

            var data = CreateSampleData();
            using var ms = new MemoryStream();
            await writer.WriteAsync(data, ms, "Transactions");

            ms.Position = 0;
            using var document = SpreadsheetDocument.Open(ms, false);
            var workbookPart = document.WorkbookPart!;
            var stylesPart = workbookPart.WorkbookStylesPart!;
            var stylesheet = stylesPart.Stylesheet;

            stylesheet.CellFormats.Should().NotBeNull();
            stylesheet.Fills.Should().NotBeNull();
            stylesheet.Fonts.Should().NotBeNull();

            // Check that stylesheet has custom cell formats generated for conditional styles
            stylesheet.CellFormats!.Count!.Value.Should().BeGreaterThan(4);

            var sheet = workbookPart.WorksheetParts.First();
            var sheetData = sheet.Worksheet.Elements<SheetData>().First();
            var rows = sheetData.Elements<Row>().ToList();

            rows.Should().HaveCount(5); // 1 header + 4 data rows

            // Row 2 (Id=1): Amount col (C2) has Amount < 0
            var c2 = rows[1].Elements<Cell>().First(c => c.CellReference?.Value == "C2");
            c2.StyleIndex.Should().NotBeNull();
            var c2StyleIdx = (int)c2.StyleIndex!.Value;
            c2StyleIdx.Should().BeGreaterThan(0);

            // Row 3 (Id=2): Amount col (C3) has Amount > 500
            var c3 = rows[2].Elements<Cell>().First(c => c.CellReference?.Value == "C3");
            c3.StyleIndex.Should().NotBeNull();
            var c3StyleIdx = (int)c3.StyleIndex!.Value;
            c3StyleIdx.Should().BeGreaterThan(0);
            c3StyleIdx.Should().NotBe(c2StyleIdx); // Different styles

            // Row 4 (Id=3): IsFlagged = true -> Column A4 should have SoftYellow row style
            var a4 = rows[3].Elements<Cell>().First(c => c.CellReference?.Value == "A4");
            a4.StyleIndex.Should().NotBeNull();
            var a4StyleIdx = (int)a4.StyleIndex!.Value;
            a4StyleIdx.Should().BeGreaterThan(0);

            // Row 4 (Id=3): Status is "Rejected" -> D4 should have SoftRed (column conditional override)
            var d4 = rows[3].Elements<Cell>().First(c => c.CellReference?.Value == "D4");
            d4.StyleIndex.Should().NotBeNull();
            var d4StyleIdx = (int)d4.StyleIndex!.Value;
            d4StyleIdx.Should().BeGreaterThan(0);
            d4StyleIdx.Should().NotBe(a4StyleIdx); // Column rule overrides row rule
        }

        [Fact]
        public async Task OpenXml_Should_Preserve_NumberFormat_Under_Conditional_Styling()
        {
            var services = new ServiceCollection();
            services.AddExcelCore(options =>
            {
                options.UseOpenXmlEngine();
                options.RegisterFromAssemblyContaining<TransactionProfile>();
            });
            var provider = services.BuildServiceProvider();
            var writer = provider.GetRequiredService<IExcelWriter>();

            var data = new List<TransactionModel>
            {
                new TransactionModel { Id = 1, Description = "Test", Amount = -99.99m, Status = "Completed" }
            };

            using var ms = new MemoryStream();
            await writer.WriteAsync(data, ms, "Sheet1");

            ms.Position = 0;
            using var document = SpreadsheetDocument.Open(ms, false);
            var stylesPart = document.WorkbookPart!.WorkbookStylesPart!;
            var stylesheet = stylesPart.Stylesheet;

            var sheet = document.WorkbookPart.WorksheetParts.First();
            var sheetData = sheet.Worksheet.Elements<SheetData>().First();
            var row2 = sheetData.Elements<Row>().Skip(1).First();
            var c2 = row2.Elements<Cell>().First(c => c.CellReference?.Value == "C2");

            int styleIdx = (int)c2.StyleIndex!.Value;
            var cellFormat = (CellFormat)stylesheet.CellFormats!.ElementAt(styleIdx);

            // NumberFormatId should be assigned and not 0
            cellFormat.NumberFormatId.Should().NotBeNull();
            cellFormat.NumberFormatId!.Value.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task OpenXml_Should_Support_AsyncEnumerable_With_ConditionalStyles()
        {
            var services = new ServiceCollection();
            services.AddExcelCore(options =>
            {
                options.UseOpenXmlEngine();
                options.RegisterFromAssemblyContaining<TransactionProfile>();
            });
            var provider = services.BuildServiceProvider();
            var writer = provider.GetRequiredService<IExcelWriter>();

            async IAsyncEnumerable<TransactionModel> GetAsyncStream([EnumeratorCancellation] CancellationToken ct = default)
            {
                await Task.Yield();
                yield return new TransactionModel { Id = 1, Description = "Async 1", Amount = -10.0m };
                yield return new TransactionModel { Id = 2, Description = "Async 2", Amount = 1500.0m };
            }

            using var ms = new MemoryStream();
            await writer.WriteAsync(GetAsyncStream(), ms, "AsyncSheet");

            ms.Position = 0;
            using var doc = SpreadsheetDocument.Open(ms, false);
            var sheetData = doc.WorkbookPart!.WorksheetParts.First().Worksheet.Elements<SheetData>().First();
            sheetData.Elements<Row>().Should().HaveCount(3); // Header + 2 data rows
        }

        [Fact]
        public async Task EPPlus_Should_Apply_ConditionalStyles_To_Cells_And_Rows()
        {
            var services = new ServiceCollection();
            services.AddExcelCore(options =>
            {
                options.UseEPPlusEngine();
                options.UseNonCommercialLicense();
                options.RegisterFromAssemblyContaining<TransactionProfile>();
            });
            var provider = services.BuildServiceProvider();
            var writer = provider.GetRequiredService<IExcelWriter>();

            var data = CreateSampleData();
            using var ms = new MemoryStream();
            await writer.WriteAsync(data, ms, "Invoices");

            ms.Position = 0;
            using var pkg = new ExcelPackage(ms);
            var ws = pkg.Workbook.Worksheets["Invoices"];
            ws.Should().NotBeNull();

            // Row 2 (Id=1): Amount is -75.50 -> C2 should have SoftRed background and bold text
            ws.Cells["C2"].Style.Fill.PatternType.Should().Be(OfficeOpenXml.Style.ExcelFillStyle.Solid);
            ws.Cells["C2"].Style.Font.Bold.Should().BeTrue();
            var c2Bg = ws.Cells["C2"].Style.Fill.BackgroundColor.Rgb;
            c2Bg.Should().EndWith(ColorHelper.ToHex(ExcelColor.SoftRed).TrimStart('#'));

            // Row 3 (Id=2): Amount is 1200.00 -> C3 should have SoftGreen background
            ws.Cells["C3"].Style.Fill.PatternType.Should().Be(OfficeOpenXml.Style.ExcelFillStyle.Solid);
            var c3Bg = ws.Cells["C3"].Style.Fill.BackgroundColor.Rgb;
            c3Bg.Should().EndWith(ColorHelper.ToHex(ExcelColor.SoftGreen).TrimStart('#'));

            // Row 4 (Id=3): IsFlagged is true -> A4 has SoftYellow row background
            var a4Bg = ws.Cells["A4"].Style.Fill.BackgroundColor.Rgb;
            a4Bg.Should().EndWith(ColorHelper.ToHex(ExcelColor.SoftYellow).TrimStart('#'));

            // Row 4 (Id=3): Status is "Rejected" -> D4 has SoftRed column background overriding row
            var d4Bg = ws.Cells["D4"].Style.Fill.BackgroundColor.Rgb;
            d4Bg.Should().EndWith(ColorHelper.ToHex(ExcelColor.SoftRed).TrimStart('#'));

            // NumberFormat on C2 should be preserved
            ws.Cells["C2"].Style.Numberformat.Format.Should().Be("$#,##0.00");
        }

        [Fact]
        public async Task EPPlus_Should_Support_AsyncEnumerable_With_ConditionalStyles()
        {
            var services = new ServiceCollection();
            services.AddExcelCore(options =>
            {
                options.UseEPPlusEngine();
                options.UseNonCommercialLicense();
                options.RegisterFromAssemblyContaining<TransactionProfile>();
            });
            var provider = services.BuildServiceProvider();
            var writer = provider.GetRequiredService<IExcelWriter>();

            async IAsyncEnumerable<TransactionModel> GetAsyncStream([EnumeratorCancellation] CancellationToken ct = default)
            {
                await Task.Yield();
                yield return new TransactionModel { Id = 1, Description = "Async 1", Amount = -50m };
                yield return new TransactionModel { Id = 2, Description = "Async 2", Amount = 999m };
            }

            using var ms = new MemoryStream();
            await writer.WriteAsync(GetAsyncStream(), ms, "AsyncInvoices");

            ms.Position = 0;
            using var pkg = new ExcelPackage(ms);
            var ws = pkg.Workbook.Worksheets["AsyncInvoices"];
            ws.Cells["C2"].Style.Font.Bold.Should().BeTrue();
            ws.Cells["C3"].Style.Fill.BackgroundColor.Rgb.Should().EndWith(ColorHelper.ToHex(ExcelColor.SoftGreen).TrimStart('#'));
        }
    }
}

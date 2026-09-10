using Exceler.Configuration;
using Exceler.Tests.Common.Fixtures;
using FluentAssertions;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Exceler.Tests.Unit.Writers
{
    public class StyledEmployeeModel
    {
        public int Id { get; set; }
        public decimal Salary { get; set; }
    }

    public class StyledEmployeeNoHeadersModel
    {
        public int Id { get; set; }
        public decimal Salary { get; set; }
    }

    public class StyledEmployeeWithHeadersProfile : ExcelProfile<StyledEmployeeModel>
    {
        public StyledEmployeeWithHeadersProfile()
        {
            Map(x => x.Id).ToColumn(1).WithHeader("Employee ID");
            Map(x => x.Salary).ToColumn(2)
                .WithHeader("Base Salary")
                .WithBackgroundColor("#E2EFDA")
                .WithNumberFormat("$#,##0.00");
        }
    }

    public class StyledEmployeeNoHeadersProfile : ExcelProfile<StyledEmployeeNoHeadersModel>
    {
        public StyledEmployeeNoHeadersProfile()
        {
            Map(x => x.Id).ToColumn(1);
            Map(x => x.Salary).ToColumn(2)
                .WithBackgroundColor("#E2EFDA")
                .WithNumberFormat("$#,##0.00");
        }
    }

    public class ExcelWriterStyleOffsetTests : ExcelerTestBase
    {
        [Fact]
        public async Task WhenExportingDataWithHeadersAndColumnStyles_StylesAreAppliedToDataRowsAndNotHeaderRow()
        {
            // Arrange
            var data = new List<StyledEmployeeModel>
            {
                new() { Id = 1, Salary = 50000m },
                new() { Id = 2, Salary = 75000m }
            };

            // Act
            var bytes = await Writer.Write(data);
            using var stream = new MemoryStream(bytes);
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            // Assert
            // Row 1 (Header) must NOT have data number formatting or data background fill
            sheet.Cells[1, 2].Text.Should().Be("Base Salary");
            sheet.Cells[1, 2].Style.Numberformat.Format.Should().NotBe("$#,##0.00");
            sheet.Cells[1, 2].Style.Fill.PatternType.Should().Be(ExcelFillStyle.None);

            // Row 2 (First data row) must have the configured number format and background fill
            sheet.Cells[2, 2].Style.Numberformat.Format.Should().Be("$#,##0.00");
            sheet.Cells[2, 2].Style.Fill.PatternType.Should().Be(ExcelFillStyle.Solid);
            sheet.Cells[2, 2].Style.Fill.BackgroundColor.Rgb.Should().EndWith("E2EFDA");

            // Row 3 (Second data row) must also have the styles
            sheet.Cells[3, 2].Style.Numberformat.Format.Should().Be("$#,##0.00");
            sheet.Cells[3, 2].Style.Fill.PatternType.Should().Be(ExcelFillStyle.Solid);
            sheet.Cells[3, 2].Style.Fill.BackgroundColor.Rgb.Should().EndWith("E2EFDA");
        }

        [Fact]
        public async Task WhenExportingDataWithoutHeaders_StylesAreAppliedStartingFromRowOne()
        {
            // Arrange
            var data = new List<StyledEmployeeNoHeadersModel>
            {
                new() { Id = 10, Salary = 30000m }
            };

            // Act
            var bytes = await Writer.Write(data);
            using var stream = new MemoryStream(bytes);
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            // Assert
            // Since there are no headers, row 1 is data and must have styles applied
            sheet.Cells[1, 2].Style.Numberformat.Format.Should().Be("$#,##0.00");
            sheet.Cells[1, 2].Style.Fill.PatternType.Should().Be(ExcelFillStyle.Solid);
        }

        [Fact]
        public async Task WhenExportingEmptyDataWithHeaders_HeaderRowIsNotModifiedByColumnStyles()
        {
            // Arrange
            var data = new List<StyledEmployeeModel>();

            // Act
            var bytes = await Writer.Write(data);
            using var stream = new MemoryStream(bytes);
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            // Assert
            sheet.Cells[1, 2].Text.Should().Be("Base Salary");
            sheet.Cells[1, 2].Style.Numberformat.Format.Should().NotBe("$#,##0.00");
            sheet.Cells[1, 2].Style.Fill.PatternType.Should().Be(ExcelFillStyle.None);
        }
    }
}

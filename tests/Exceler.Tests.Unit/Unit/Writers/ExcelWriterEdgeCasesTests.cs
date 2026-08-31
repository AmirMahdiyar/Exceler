using Exceler.Tests.Infrastructure.Base;
using Exceler.Tests.Infrastructure.EdgeCases;
using Exceler.Tests.Infrastructure.ModelOfTest;
using FluentAssertions;
using OfficeOpenXml;

namespace Exceler.Tests.Unit.Writers
{
    public class ExcelWriterEdgeCasesTests : ExcelerTestBase
    {

        [Fact]
        public async Task Write_exports_only_headers_when_input_list_is_empty()
        {
            // Arrange
            var emptyList = new List<TestModel>();

            // Act
            byte[] excelBytes = await Writer.Write(emptyList);

            // Assert
            using var stream = new MemoryStream(excelBytes);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];

            worksheet.Dimension.Rows.Should().Be(1);

            worksheet.Cells[1, 1].Text.Should().Be("User ID");
            worksheet.Cells[1, 2].Text.Should().Be("Full Name");
        }

        [Fact]
        public async Task Write_handles_null_values_gracefully_without_throwing_exceptions()
        {
            // Arrange
            var data = new List<EdgeCaseModel>
            {
                new EdgeCaseModel
                {
                    NullableInt = null,
                    IsActive = true,
                    Status = TestStatus.Pending
                }
            };

            // Act
            byte[] excelBytes = await Writer.Write(data);

            // Assert
            using var stream = new MemoryStream(excelBytes);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];

            worksheet.Cells[2, 1].Value.Should().BeNull();

            worksheet.Cells[2, 2].Value.Should().Be(true);
        }
    }
}

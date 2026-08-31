using Exceler.Tests.Infrastructure;
using Exceler.Tests.Infrastructure.Base;
using Exceler.Tests.Infrastructure.ModelOfTest;
using FluentAssertions;
using OfficeOpenXml;

namespace Exceler.Tests.Unit.Writers
{
    public class ExcelProfileBehaviorTests : ExcelerTestBase
    {
        [Fact]
        public async Task Profile_configurations_are_applied_correctly_to_exported_excel_file()
        {
            // Arrange
            var model = new TestModelBuilder()
                .WithId(105)
                .WithFullName("Vladimir Khorikov")
                .Build();

            var data = new List<TestModel> { model };

            //Act
            byte[] excelBytes = await Writer.Write(data);

            // Assert
            using var stream = new MemoryStream(excelBytes);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];

            worksheet.Cells[1, 1].Text.Should().Be("User ID");
            worksheet.Cells[1, 2].Text.Should().Be("Full Name");

            worksheet.Cells[1, 2].Style.Font.Bold.Should().BeTrue();

            worksheet.Cells[2, 1].Value.Should().Be(105);
            worksheet.Cells[2, 2].Text.Should().Be("Vladimir Khorikov");
        }

        [Fact]
        public async Task Profile_trims_string_values_when_trimming_is_enabled()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("Sheet1");
            using var stream = builder
                .WithTestModelHeaders()
                .WithRow(2, 105, "   Vladimir Khorikov   ", 5000m, new DateTime(2026, 1, 1))
                .Build();

            // Act
            var results = Reader.Read<TestModel, TestModel>(stream).ToList();

            // Assert
            results.Should().ContainSingle();
            results.First().IsValid.Should().BeTrue();
            results.First().Data!.FullName.Should().Be("Vladimir Khorikov");
        }
    }
}

using Exceler.Tests.Infrastructure;
using Exceler.Tests.Infrastructure.Base;
using Exceler.Tests.Infrastructure.ModelOfTest;
using FluentAssertions;

namespace Exceler.Tests.Unit.Readers
{
    public class ExcelReaderStructureTests : ExcelerTestBase
    {

        [Fact]
        public void Read_returns_empty_list_when_file_has_headers_but_no_data()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("EmptyData");
            using var stream = builder.WithTestModelHeaders().Build();

            // Act
            var results = Reader.Read<TestModel, TestModel>(stream).ToList();

            // Assert
            results.Should().BeEmpty("because the file only contains a header row");
        }

        [Fact]
        public void Read_ignores_extra_unmapped_columns_without_throwing_exceptions()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("ExtraColumns");
            using var stream = builder
                .WithTestModelHeaders()
                .WithCell(1, 5, "Extra Unmapped Header")
                .WithRow(2, 1, "John Doe", null, null, "Should be ignored")
                .Build();

            // Act
            var results = Reader.Read<TestModel, TestModel>(stream).ToList();

            // Assert
            results.Should().ContainSingle();
            results[0].IsValid.Should().BeTrue();
            results[0].Data!.FullName.Should().Be("John Doe");
        }

        [Fact]
        public void Read_recovers_successfully_when_there_are_completely_empty_rows_in_between_data()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("EmptyRows");
            using var stream = builder
                .WithTestModelHeaders()
                .WithCell(2, 1, 1)
                .WithCell(4, 1, 2)
                .Build();

            // Act
            var results = Reader.Read<TestModel, TestModel>(stream).Where(r => r.IsValid).ToList();

            // Assert
            results.Should().HaveCount(2, "because empty rows should not stop the pipeline from reading subsequent valid rows");
            results[0].Data!.Id.Should().Be(1);
            results[1].Data!.Id.Should().Be(2);
        }
        [Fact]
        public void Read_throws_exception_when_file_is_not_a_valid_excel_format()
        {
            var invalidBytes = System.Text.Encoding.UTF8.GetBytes("This is just a plain text file, not an Excel file!");
            using var stream = new MemoryStream(invalidBytes);

            // Act
            Action act = () => Reader.Read<TestModel, TestModel>(stream).ToList();

            // Assert
            act.Should().Throw<InvalidDataException>();
        }
        [Fact]
        public void Read_extracts_calculated_value_from_cells_with_formulas()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("Formulas");
            using var stream = builder
                .WithTestModelHeaders()
                .WithCell(2, 1, 1)
                .WithCell(2, 2, "Formula User")
                .WithFormula(2, 3, "SUM(1000, 500)")
                .WithCell(2, 4, "2026-01-01")
                .Build();

            // Act
            var results = Reader.Read<TestModel, TestModel>(stream).ToList();

            // Assert
            results.Should().ContainSingle();
            results.First().Data!.Balance.Should().Be(1500m);
        }
        [Fact]
        public void Read_handles_duplicate_headers_gracefully_by_picking_the_first_match()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("DupHeaders");
            using var stream = builder
                .WithTestModelHeaders()
                .WithCell(1, 5, "Full Name")
                .WithRow(2, 1, "First Name (Expected)", 1000m, "2026-01-01", "Second Name (Ignored)")
                .Build();

            // Act
            var results = Reader.Read<TestModel, TestModel>(stream).ToList();

            // Assert
            results.Should().ContainSingle();
            results.First().IsValid.Should().BeTrue();
            results.First().Data!.FullName.Should().Be("First Name (Expected)");
        }
    }
}

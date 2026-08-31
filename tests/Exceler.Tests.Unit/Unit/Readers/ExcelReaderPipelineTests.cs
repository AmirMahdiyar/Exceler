using Exceler.Tests.Infrastructure;
using Exceler.Tests.Infrastructure.Base;
using Exceler.Tests.Infrastructure.ModelOfTest;
using FluentAssertions;

namespace Exceler.Tests.Unit.Readers
{
    public class ExcelReaderPipelineTests : ExcelerTestBase
    {

        [Fact]
        public void Pipeline_collects_multiple_format_errors_for_a_single_row_instead_of_short_circuiting()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("MultipleErrors");
            using var stream = builder
                .WithTestModelHeaders()
                .WithRow(2, 1, "John Doe", "Not_A_Number", "Not_A_Date")
                .Build();

            // Act
            var results = Reader.Read<TestModel, TestModel>(stream).ToList();

            // Assert
            results.Should().ContainSingle();
            var result = results.First();

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(2);
            result.Errors.Should().Contain(e => e.Contains("[Account Balance]"));
            result.Errors.Should().Contain(e => e.Contains("[Creation Date]"));
        }

        [Fact]
        public void Pipeline_processes_mix_of_valid_and_invalid_rows_independently()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("MixedRows");
            using var stream = builder
                .WithTestModelHeaders()
                .WithRow(2, 1, "Valid User 1", 1000m, "2026-01-01")
                .WithRow(3, 2, null, "BAD_NUMBER")
                .WithRow(4, 3, "Valid User 2", 2000m, "2026-02-02")
                .Build();

            // Act
            var results = Reader.Read<TestModel, TestModel>(stream).ToList();

            // Assert
            results.Should().HaveCount(3);
            results[0].IsValid.Should().BeTrue();
            results[0].Data!.Id.Should().Be(1);

            results[1].IsValid.Should().BeFalse();
            results[1].Data.Should().BeNull();

            results[2].IsValid.Should().BeTrue();
            results[2].Data!.Id.Should().Be(3);
        }
    }
}

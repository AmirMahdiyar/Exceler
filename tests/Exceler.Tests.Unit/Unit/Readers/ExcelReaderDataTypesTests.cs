using Exceler.Tests.Infrastructure;
using Exceler.Tests.Infrastructure.Base;
using Exceler.Tests.Infrastructure.EdgeCases;
using Exceler.Tests.Infrastructure.ModelOfTest;
using FluentAssertions;

namespace Exceler.Tests.Unit.Readers
{
    public class ExcelReaderDataTypesTests : ExcelerTestBase
    {

        [Fact]
        public void Read_parses_various_valid_formats_for_complex_types_correctly()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("Data");
            using var stream = builder
                .WithHeaders("Nullable Int", "Is Active", "Status", "Identifier", "Ratio")
                .WithRow(2, 10, "True", "Approved", Guid.NewGuid().ToString(), 15.5)
                .WithRow(3, null, "1", "reJecTed", Guid.NewGuid().ToString(), "20.75")
                .Build();

            // Act
            var results = Reader.Read<EdgeCaseModel, EdgeCaseModel>(stream).ToList();

            // Assert
            results.Should().HaveCount(2);
            results.Should().OnlyContain(r => r.IsValid);

            // Assert Row 2
            results[0].Data!.NullableInt.Should().Be(10);
            results[0].Data.IsActive.Should().BeTrue();
            results[0].Data.Status.Should().Be(TestStatus.Approved);

            // Assert Row 3
            results[1].Data!.NullableInt.Should().BeNull();
            results[1].Data.IsActive.Should().BeTrue();
            results[1].Data.Status.Should().Be(TestStatus.Rejected);
            results[1].Data.Ratio.Should().Be(20.75);
        }

        [Fact]
        public void Read_handles_DateTime_formats_including_OADate_and_strings()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("Dates");
            using var stream = builder
                .WithTestModelHeaders()
                .WithCell(2, 1, 1).WithCell(2, 4, 45000.5)               // OADate Format
                .WithCell(3, 1, 2).WithCell(3, 4, "2026-08-31 14:30")  // String Format
                .Build();

            // Act
            var results = Reader.Read<TestModel, TestModel>(stream).ToList();

            // Assert
            results.Should().HaveCount(2);
            results.All(r => r.IsValid).Should().BeTrue();

            results[0].Data!.CreatedAt.Should().Be(DateTime.FromOADate(45000.5));
            results[1].Data!.CreatedAt.Should().Be(new DateTime(2026, 8, 31, 14, 30, 0));
        }
    }
}

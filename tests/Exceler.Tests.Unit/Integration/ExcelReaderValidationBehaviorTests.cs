using Exceler.Core.Exceptions;
using Exceler.Tests.Infrastructure;
using Exceler.Tests.Infrastructure.Base;
using Exceler.Tests.Infrastructure.ModelOfTest;
using FluentAssertions;

namespace Exceler.Tests.Integration
{
    public class ExcelReaderValidationBehaviorTests : ExcelerTestBase
    {

        [Fact]
        public void Read_throws_ExcelTemplateMismatchException_when_headers_do_not_match_profile()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("Sheet1");
            using var stream = builder
                .WithHeaders("Wrong Header 1", "Wrong Header 2")
                .Build();

            // Act
            Action act = () => Reader.Read<TestModel, TestModel>(stream).ToList();

            // Assert
            act.Should().Throw<ExcelTemplateMismatchException>()
               .Which.MissingOrInvalidHeaders.Should().NotBeEmpty();
        }

        [Fact]
        public void Read_adds_format_error_to_result_when_cell_data_cannot_be_cast_safely()
        {
            {
                // Arrange
                using var builder = new ExcelStreamBuilder("Sheet1");
                using var stream = builder
                    .WithTestModelHeaders()
                    .WithRow(2, 1, "Valid Name", "Not_A_Number", "2026-01-01")
                    .Build();

                // Act
                var results = Reader.Read<TestModel, TestModel>(stream).ToList();

                // Assert
                results.Should().ContainSingle();
                var result = results.First();

                result.IsValid.Should().BeFalse();
                result.Errors.Should().Contain(err => err.Contains("Format of [Account Balance] Column is incorrect"));
            }
        }

        [Fact]
        public void Read_adds_business_validation_errors_to_result_when_validator_fails()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("Sheet1");
            using var stream = builder
                .WithTestModelHeaders()
                .WithRow(2, 1, "Valid Name", -500m, "2026-01-01")
                .Build();

            // Act
            var results = Reader.Read<TestModel, TestModel>(stream).ToList();

            // Assert
            results.Should().ContainSingle();
            var result = results.First();

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Balance cannot be negative.");
        }
    }
}

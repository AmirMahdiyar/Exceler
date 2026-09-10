using Exceler.Abstractions;
using Exceler.Configuration;
using Exceler.Tests.Common.Fixtures;
using Exceler.Tests.Common.Fixtures;
using FluentAssertions;
using System;
using System.Linq;

namespace Exceler.Tests.Unit.Readers
{
    public class ExceptionTestModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class ThrowingCodeConverter : IExcelValueConverter<string>
    {
        public string ConvertFromExcel(object? value)
        {
            var str = value?.ToString();
            if (str == "INVALID_FORMAT")
                throw new FormatException("Invalid custom code format");

            return str ?? string.Empty;
        }

        public object? ConvertToExcel(string? value) => value;
    }

    public class ExceptionTestProfile : ExcelProfile<ExceptionTestModel>
    {
        public ExceptionTestProfile()
        {
            Map(x => x.Id).ToColumn(1).WithHeader("Item ID");
            Map(x => x.Code).ToColumn(2).WithHeader("Item Code").WithConverter(new ThrowingCodeConverter());
            Map(x => x.Value).ToColumn(3).WithHeader("Item Value");
        }
    }

    public class ExceptionTestProcessor : IExcelProcessor<ExceptionTestModel, ExceptionTestModel>
    {
        public ExceptionTestModel Process(ExceptionTestModel input) => input;
    }

    public class StrictSetterModel
    {
        public int Id { get; set; }

        private int _age;
        public int Age
        {
            get => _age;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Age cannot be negative");
                _age = value;
            }
        }
    }

    public class StrictSetterProfile : ExcelProfile<StrictSetterModel>
    {
        public StrictSetterProfile()
        {
            Map(x => x.Id).ToColumn(1).WithHeader("User ID");
            Map(x => x.Age).ToColumn(2).WithHeader("User Age");
        }
    }

    public class StrictSetterProcessor : IExcelProcessor<StrictSetterModel, StrictSetterModel>
    {
        public StrictSetterModel Process(StrictSetterModel input) => input;
    }

    public class ExcelReaderExceptionHandlingTests : ExcelerTestBase
    {
        [Fact]
        public void WhenCustomConverterThrowsFormatException_RowIsMarkedInvalidAndSubsequentRowsAreReadSuccessfully()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("CustomConverterErrors");
            using var stream = builder
                .WithHeaders("Item ID", "Item Code", "Item Value")
                .WithRow(2, 1, "VALID-01", 100)
                .WithRow(3, 2, "INVALID_FORMAT", 200)
                .WithRow(4, 3, "VALID-02", 300)
                .Build();

            // Act
            var results = Reader.Read<ExceptionTestModel, ExceptionTestModel>(stream).ToList();

            // Assert
            results.Should().HaveCount(3);

            results[0].IsValid.Should().BeTrue();
            results[0].Data!.Code.Should().Be("VALID-01");

            results[1].IsValid.Should().BeFalse();
            results[1].Data.Should().BeNull();
            results[1].Errors.Should().Contain(e => e.Contains("[Item Code]") && e.Contains("Invalid custom code format"));

            results[2].IsValid.Should().BeTrue();
            results[2].Data!.Code.Should().Be("VALID-02");
        }

        [Fact]
        public void WhenPropertySetterThrowsArgumentOutOfRangeException_RowIsMarkedInvalidWithDescriptiveErrorMessage()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("StrictSetterErrors");
            using var stream = builder
                .WithHeaders("User ID", "User Age")
                .WithRow(2, 1, 25)
                .WithRow(3, 2, -10)
                .WithRow(4, 3, 40)
                .Build();

            // Act
            var results = Reader.Read<StrictSetterModel, StrictSetterModel>(stream).ToList();

            // Assert
            results.Should().HaveCount(3);

            results[0].IsValid.Should().BeTrue();
            results[0].Data!.Age.Should().Be(25);

            results[1].IsValid.Should().BeFalse();
            results[1].Data.Should().BeNull();
            results[1].Errors.Should().Contain(e => e.Contains("[User Age]") && e.Contains("Age cannot be negative"));

            results[2].IsValid.Should().BeTrue();
            results[2].Data!.Age.Should().Be(40);
        }

        [Fact]
        public void WhenSafeConverterFailsToCastValue_StandardFormatErrorMessageIsRetained()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("StandardCastErrors");
            using var stream = builder
                .WithHeaders("User ID", "User Age")
                .WithRow(2, 1, "NotAnInteger")
                .Build();

            // Act
            var results = Reader.Read<StrictSetterModel, StrictSetterModel>(stream).ToList();

            // Assert
            results.Should().ContainSingle();
            var result = results.First();

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Format of [User Age] Column is incorrect");
        }
    }
}

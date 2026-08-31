using Exceler.Abstractions;
using FluentAssertions;

namespace Exceler.Tests.Unit.Converter
{
    public class DelimitedListConverter : IExcelValueConverter<List<string>>
    {
        private readonly string _delimiter;
        public DelimitedListConverter(string delimiter = ",") => _delimiter = delimiter;

        public List<string>? ConvertFromExcel(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return new List<string>();

            return value.ToString()!
                .Split(_delimiter, System.StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
        }

        public object? ConvertToExcel(List<string>? value)
        {
            if (value == null || !value.Any()) return null;
            return string.Join(_delimiter, value);
        }
    }

    // 2. Output-Based Unit Tests
    public class DelimitedListConverterTests
    {
        private readonly DelimitedListConverter _sut = new();

        [Theory]
        [InlineData(null, 0)]
        [InlineData("", 0)]
        [InlineData("   ", 0)]
        [InlineData("C#, .NET", 2)]
        [InlineData("C# ,  , .NET ", 2)]
        [InlineData("SingleItem", 1)]
        public void ConvertFromExcel_parses_delimited_strings_into_clean_lists(object? input, int expectedCount)
        {
            // Act
            var result = _sut.ConvertFromExcel(input);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedCount);
        }

        [Fact]
        public void ConvertFromExcel_trims_whitespace_from_individual_elements()
        {
            // Arrange
            object input = "  Clean Architecture , Unit Testing  ";

            // Act
            var result = _sut.ConvertFromExcel(input);

            // Assert
            result.Should().ContainInOrder("Clean Architecture", "Unit Testing");
        }

        [Fact]
        public void ConvertToExcel_joins_list_elements_using_configured_delimiter()
        {
            // Arrange
            var input = new List<string> { "AAA", "BBB" };

            // Act
            var result = _sut.ConvertToExcel(input);

            // Assert
            result.Should().Be("AAA,BBB");
        }
    }
}

using Exceler.Tests.Common.TestDoubles.Converters;
using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace Exceler.Tests.Unit.Converter
{
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

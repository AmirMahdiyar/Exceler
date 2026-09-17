using Exceler.Core;
using FluentAssertions;
using System;
using Xunit;

namespace Exceler.Tests.Unit.Unit.Writers
{
    public class SpreadsheetUtilsTests
    {
        [Theory]
        [InlineData(1, "A")]
        [InlineData(2, "B")]
        [InlineData(26, "Z")]
        [InlineData(27, "AA")]
        [InlineData(28, "AB")]
        [InlineData(52, "AZ")]
        [InlineData(53, "BA")]
        [InlineData(702, "ZZ")]
        [InlineData(703, "AAA")]
        [InlineData(16384, "XFD")] // Maximum columns in Excel
        public void GetColumnLetter_ValidIndex_ReturnsExpectedLetters(int columnIndex, string expectedLetter)
        {
            // Act
            string letter = SpreadsheetUtils.GetColumnLetter(columnIndex);

            // Assert
            letter.Should().Be(expectedLetter);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void GetColumnLetter_InvalidIndex_ThrowsArgumentOutOfRangeException(int columnIndex)
        {
            // Act
            Action act = () => SpreadsheetUtils.GetColumnLetter(columnIndex);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(null, "Sheet1")]
        [InlineData("", "Sheet1")]
        [InlineData("   ", "Sheet1")]
        [InlineData("NormalSheet", "NormalSheet")]
        [InlineData("S:1*2?3/4\\5[6]7", "S_1_2_3_4_5_6_7")]
        [InlineData("'WrappedInQuotes'", "WrappedInQuotes")]
        public void SanitizeSheetName_VariousInputs_ReturnsSanitizedString(string? input, string expected)
        {
            // Act
            string result = SpreadsheetUtils.SanitizeSheetName(input);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void SanitizeSheetName_Exceeds31Characters_TruncatesTo31Characters()
        {
            // Arrange
            string longName = new string('A', 50);

            // Act
            string result = SpreadsheetUtils.SanitizeSheetName(longName);

            // Assert
            result.Should().HaveLength(31);
            result.Should().Be(new string('A', 31));
        }

        [Fact]
        public void SanitizeSheetName_OnlyQuotesAndWhitespace_DefaultsToSheet1()
        {
            // Arrange
            string weirdInput = "  '''  ";

            // Act
            string result = SpreadsheetUtils.SanitizeSheetName(weirdInput);

            // Assert
            result.Should().Be("Sheet1");
        }
    }
}

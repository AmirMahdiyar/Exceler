using Exceler.Core;
using FluentAssertions;
using System.Drawing;

namespace Exceler.Tests.Unit.Core
{
    public class ColorHelperTests
    {
        [Fact]
        public void WhenParsingSixDigitHexColorWithHash_ColorIsParsedCorrectly()
        {
            // Act
            var color = ColorHelper.FromHex("#FF0000");

            // Assert
            color.R.Should().Be(255);
            color.G.Should().Be(0);
            color.B.Should().Be(0);
            color.A.Should().Be(255);
        }

        [Fact]
        public void WhenParsingSixDigitHexColorWithoutHash_ColorIsParsedCorrectly()
        {
            // Act
            var color = ColorHelper.FromHex("00FF00");

            // Assert
            color.R.Should().Be(0);
            color.G.Should().Be(255);
            color.B.Should().Be(0);
            color.A.Should().Be(255);
        }

        [Fact]
        public void WhenParsingEightDigitHexColorWithAlpha_AlphaAndColorAreParsedCorrectly()
        {
            // Act
            var color = ColorHelper.FromHex("#800000FF");

            // Assert
            color.A.Should().Be(128);
            color.R.Should().Be(0);
            color.G.Should().Be(0);
            color.B.Should().Be(255);
        }

        [Fact]
        public void WhenParsingThreeDigitShorthandHex_ColorIsExpandedAndParsedCorrectly()
        {
            // Act
            var color = ColorHelper.FromHex("#FFF");

            // Assert
            color.R.Should().Be(255);
            color.G.Should().Be(255);
            color.B.Should().Be(255);
            color.A.Should().Be(255);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WhenParsingNullOrWhitespaceHex_ArgumentExceptionIsThrown(string? invalidHex)
        {
            // Act
            Action act = () => ColorHelper.FromHex(invalidHex);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("#GG0000")]
        [InlineData("#12345")]
        [InlineData("NotAColor")]
        public void WhenParsingMalformedHex_FormatExceptionIsThrown(string malformedHex)
        {
            // Act
            Action act = () => ColorHelper.FromHex(malformedHex);

            // Assert
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void WhenConvertingColorToHex_CorrectHexFormatIsReturned()
        {
            // Arrange
            var color = Color.FromArgb(255, 255, 0, 0);

            // Act
            var hex = ColorHelper.ToHex(color);

            // Assert
            hex.Should().Be("#FF0000");
        }

        [Theory]
        [InlineData("Blue", (byte)0, (byte)0, (byte)255)]
        [InlineData("red", (byte)255, (byte)0, (byte)0)]
        [InlineData("SoftGreen", (byte)226, (byte)239, (byte)218)]
        [InlineData("DarkRed", (byte)192, (byte)0, (byte)0)]
        public void WhenParsingExcelColorNameAsString_ColorIsParsedCorrectly(string colorName, byte expectedR, byte expectedG, byte expectedB)
        {
            // Act
            var color = ColorHelper.FromHex(colorName);

            // Assert
            color.R.Should().Be(expectedR);
            color.G.Should().Be(expectedG);
            color.B.Should().Be(expectedB);
        }

        [Fact]
        public void WhenConvertingExcelColorEnumToHex_CorrectHexFormatIsReturned()
        {
            // Act
            var hex = ColorHelper.ToHex(Exceler.Configuration.ExcelColor.SoftBlue);

            // Assert
            hex.Should().Be("#DDEBF7");
        }
    }
}

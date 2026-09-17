using Exceler.Configuration;
using Exceler.Core;
using FluentAssertions;
using System;
using System.Drawing;
using Xunit;

namespace Exceler.Tests.Unit.Unit.Common
{
    public class ColorHelperAndStyleBranchTests
    {
        [Fact]
        public void ColorHelper_AllExcelColors_ConvertSuccessfully()
        {
            foreach (ExcelColor color in Enum.GetValues(typeof(ExcelColor)))
            {
                string hex = ColorHelper.ToHex(color);
                hex.Should().StartWith("#");
                hex.Should().HaveLength(7);
            }

            // Unknown enum value fallback
            ColorHelper.ToHex((ExcelColor)9999).Should().Be("#000000");
        }

        [Theory]
        [InlineData("#abc", 170, 187, 204)]
        [InlineData("#ABC", 170, 187, 204)]
        [InlineData("#123", 17, 34, 51)]
        [InlineData("#112233", 17, 34, 51)]
        [InlineData("#80112233", 17, 34, 51)]
        [InlineData("Black", 0, 0, 0)]
        [InlineData("AliceBlue", 240, 248, 255)]
        [InlineData("Crimson", 220, 20, 60)]
        public void ColorHelper_FromHex_ParsesValidFormats(string colorStr, int expectedR, int expectedG, int expectedB)
        {
            var color = ColorHelper.FromHex(colorStr);
            color.R.Should().Be((byte)expectedR);
            color.G.Should().Be((byte)expectedG);
            color.B.Should().Be((byte)expectedB);
        }

        [Fact]
        public void ColorHelper_ToHex_WithAlpha_Returns8CharHex()
        {
            var transparentRed = Color.FromArgb(128, 255, 0, 0);
            string hex = ColorHelper.ToHex(transparentRed);
            hex.Should().Be("#80FF0000");
        }

        [Theory]
        [InlineData("NotAColor")]
        [InlineData("#XYZ")]
        [InlineData("#12")]
        [InlineData("#12345")]
        [InlineData("#1234567")]
        public void ColorHelper_FromHex_Invalid_ThrowsFormatException(string invalid)
        {
            Action act = () => ColorHelper.FromHex(invalid);
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void ColumnStyle_NullableColors_BranchCoverage()
        {
            var style = new ColumnStyle();

            // Initial state
            style.BackgroundColor.Should().BeNull();
            style.FontColor.Should().BeNull();

            // Set color
            style.BackgroundColor = Color.Red;
            style.FontColor = Color.Blue;
            style.BackgroundColorHex.Should().Be("#FF0000");
            style.FontColorHex.Should().Be("#0000FF");

            // Set null
            style.BackgroundColor = null;
            style.FontColor = null;
            style.BackgroundColorHex.Should().BeNull();
            style.FontColorHex.Should().BeNull();

            // Empty hex string
            style.BackgroundColorHex = "";
            style.FontColorHex = "";
            style.BackgroundColor.Should().BeNull();
            style.FontColor.Should().BeNull();
        }
    }
}

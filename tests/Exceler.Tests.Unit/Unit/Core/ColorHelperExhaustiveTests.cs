using Exceler.Configuration;
using Exceler.Core;
using FluentAssertions;
using System.Drawing;

namespace Exceler.Tests.Unit.Core
{
    public class ColorHelperExhaustiveTests
    {
        [Theory]
        [InlineData(ExcelColor.Black, "#000000")]
        [InlineData(ExcelColor.White, "#FFFFFF")]
        [InlineData(ExcelColor.Red, "#FF0000")]
        [InlineData(ExcelColor.DarkRed, "#C00000")]
        [InlineData(ExcelColor.Green, "#008000")]
        [InlineData(ExcelColor.DarkGreen, "#375623")]
        [InlineData(ExcelColor.Blue, "#0000FF")]
        [InlineData(ExcelColor.DarkBlue, "#1F4E78")]
        [InlineData(ExcelColor.Navy, "#000080")]
        [InlineData(ExcelColor.Yellow, "#FFFF00")]
        [InlineData(ExcelColor.Gold, "#FFD700")]
        [InlineData(ExcelColor.Orange, "#FFA500")]
        [InlineData(ExcelColor.DarkOrange, "#ED7D31")]
        [InlineData(ExcelColor.Purple, "#800080")]
        [InlineData(ExcelColor.Teal, "#008080")]
        [InlineData(ExcelColor.Cyan, "#00FFFF")]
        [InlineData(ExcelColor.Gray, "#808080")]
        [InlineData(ExcelColor.DarkGray, "#595959")]
        [InlineData(ExcelColor.LightGray, "#D9D9D9")]
        [InlineData(ExcelColor.SoftGreen, "#E2EFDA")]
        [InlineData(ExcelColor.SoftRed, "#FCE4D6")]
        [InlineData(ExcelColor.SoftBlue, "#DDEBF7")]
        [InlineData(ExcelColor.SoftYellow, "#FFF2CC")]
        [InlineData(ExcelColor.SoftGray, "#F2F2F2")]
        public void Predefined_excel_color_is_converted_to_exact_hex_code(ExcelColor color, string expectedHex)
        {
            var sut = color;

            var result = ColorHelper.ToHex(sut);

            result.Should().Be(expectedHex);
        }

        [Fact]
        public void Unknown_excel_color_enum_defaults_to_black_hex()
        {
            var sut = (ExcelColor)9999;

            var result = ColorHelper.ToHex(sut);

            result.Should().Be("#000000");
        }

        [Theory]
        [InlineData("#F00", 255, 0, 0)]
        [InlineData("#0F0", 0, 255, 0)]
        [InlineData("#00F", 0, 0, 255)]
        [InlineData("F00", 255, 0, 0)]
        public void Three_digit_shorthand_hex_is_expanded_to_full_color(string hex, int r, int g, int b)
        {
            var sut = hex;

            var result = ColorHelper.FromHex(sut);

            result.R.Should().Be((byte)r);
            result.G.Should().Be((byte)g);
            result.B.Should().Be((byte)b);
        }

        [Theory]
        [InlineData("red", 255, 0, 0)]
        [InlineData("green", 0, 128, 0)]
        [InlineData("blue", 0, 0, 255)]
        [InlineData("black", 0, 0, 0)]
        [InlineData("white", 255, 255, 255)]
        public void Named_color_string_is_parsed_to_matching_drawing_color(string colorName, int r, int g, int b)
        {
            var sut = colorName;

            var result = ColorHelper.FromHex(sut);

            result.R.Should().Be((byte)r);
            result.G.Should().Be((byte)g);
            result.B.Should().Be((byte)b);
        }

        [Theory]
        [InlineData("#80402010", 128, 64, 32, 16)]
        [InlineData("#40802010", 64, 128, 32, 16)]
        public void Eight_digit_argb_hex_string_is_parsed_to_color_with_alpha(string hex, int a, int r, int g, int b)
        {
            var sut = hex;

            var result = ColorHelper.FromHex(sut);

            result.A.Should().Be((byte)a);
            result.R.Should().Be((byte)r);
            result.G.Should().Be((byte)g);
            result.B.Should().Be((byte)b);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Null_or_whitespace_color_string_throws_ArgumentException(string? emptyInput)
        {
            var sut = emptyInput;

            var act = () => ColorHelper.FromHex(sut!);

            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("#12")]
        [InlineData("#12345")]
        [InlineData("#GGGGGG")]
        [InlineData("unknown_nonexistent_color_name")]
        public void Malformed_color_string_throws_FormatException(string invalidInput)
        {
            var sut = invalidInput;

            var act = () => ColorHelper.FromHex(sut);

            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void System_drawing_color_with_alpha_is_converted_to_eight_digit_hex()
        {
            var sut = Color.FromArgb(128, 64, 32, 16);

            var result = ColorHelper.ToHex(sut);

            result.Should().Be("#80402010");
        }

        [Fact]
        public void System_drawing_color_without_alpha_is_converted_to_six_digit_hex()
        {
            var sut = Color.FromArgb(255, 64, 32, 16);

            var result = ColorHelper.ToHex(sut);

            result.Should().Be("#402010");
        }

        [Fact]
        public void Known_named_drawing_color_is_converted_to_hex()
        {
            var sut = Color.DarkSlateBlue;

            var result = ColorHelper.ToHex(sut);

            result.Should().Be("#483D8B");
        }
    }
}

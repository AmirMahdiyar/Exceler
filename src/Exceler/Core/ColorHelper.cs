using Exceler.Configuration;
using System.Drawing;
using System.Globalization;

namespace Exceler.Core
{
    /// <summary>
    /// Provides platform-agnostic, zero-allocation hex color parsing utilities for Excel styling.
    /// Supports Hex (#RGB, #RRGGBB, #AARRGGBB), ExcelColor enum names, and standard CSS color names.
    /// Fully compatible with Windows, Linux, and Docker environments.
    /// </summary>
    internal static class ColorHelper
    {
        public static Color FromHex(string? hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                throw new ArgumentException("Color string cannot be null or empty.", nameof(hex));

            var trimmed = hex.Trim();

            // 1. Check if it matches an ExcelColor enum name (case-insensitive, e.g. "blue", "SoftGreen")
            if (Enum.TryParse<ExcelColor>(trimmed, ignoreCase: true, out var excelColor))
            {
                trimmed = ToHex(excelColor);
            }
            // 2. Check if it's a known System.Drawing/CSS color name (e.g. "Crimson", "AliceBlue")
            else if (!trimmed.StartsWith("#"))
            {
                var namedColor = Color.FromName(trimmed);
                if (namedColor.IsKnownColor && (namedColor.A != 0 || namedColor.R != 0 || namedColor.G != 0 || namedColor.B != 0 || string.Equals(trimmed, "Black", StringComparison.OrdinalIgnoreCase)))
                {
                    return namedColor;
                }
            }

            var span = trimmed.AsSpan();
            if (span.StartsWith("#"))
                span = span.Slice(1);

            if (span.Length == 6)
            {
                if (byte.TryParse(span.Slice(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte r) &&
                    byte.TryParse(span.Slice(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte g) &&
                    byte.TryParse(span.Slice(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b))
                {
                    return Color.FromArgb(255, r, g, b);
                }
            }
            else if (span.Length == 8)
            {
                if (byte.TryParse(span.Slice(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte a) &&
                    byte.TryParse(span.Slice(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte r) &&
                    byte.TryParse(span.Slice(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte g) &&
                    byte.TryParse(span.Slice(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b))
                {
                    return Color.FromArgb(a, r, g, b);
                }
            }
            else if (span.Length == 3)
            {
                if (IsHexDigit(span[0]) && IsHexDigit(span[1]) && IsHexDigit(span[2]))
                {
                    byte r = ConvertHexNibble(span[0]);
                    byte g = ConvertHexNibble(span[1]);
                    byte b = ConvertHexNibble(span[2]);
                    return Color.FromArgb(255, (r << 4) | r, (g << 4) | g, (b << 4) | b);
                }
            }

            throw new FormatException($"Invalid color string: '{hex}'. Expected a known color name, or '#RGB', '#RRGGBB', '#AARRGGBB'.");
        }

        public static string ToHex(ExcelColor color) => color switch
        {
            ExcelColor.Black => "#000000",
            ExcelColor.White => "#FFFFFF",
            ExcelColor.Red => "#FF0000",
            ExcelColor.DarkRed => "#C00000",
            ExcelColor.Green => "#008000",
            ExcelColor.DarkGreen => "#375623",
            ExcelColor.Blue => "#0000FF",
            ExcelColor.DarkBlue => "#1F4E78",
            ExcelColor.Navy => "#000080",
            ExcelColor.Yellow => "#FFFF00",
            ExcelColor.Gold => "#FFD700",
            ExcelColor.Orange => "#FFA500",
            ExcelColor.DarkOrange => "#ED7D31",
            ExcelColor.Purple => "#800080",
            ExcelColor.Teal => "#008080",
            ExcelColor.Cyan => "#00FFFF",
            ExcelColor.Gray => "#808080",
            ExcelColor.DarkGray => "#595959",
            ExcelColor.LightGray => "#D9D9D9",

            // Pastel backgrounds
            ExcelColor.SoftGreen => "#E2EFDA",
            ExcelColor.SoftRed => "#FCE4D6",
            ExcelColor.SoftBlue => "#DDEBF7",
            ExcelColor.SoftYellow => "#FFF2CC",
            ExcelColor.SoftGray => "#F2F2F2",

            _ => "#000000"
        };

        public static string ToHex(Color color)
        {
            return color.A == 255
                ? $"#{color.R:X2}{color.G:X2}{color.B:X2}"
                : $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private static bool IsHexDigit(char c)
        {
            return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
        }

        private static byte ConvertHexNibble(char c)
        {
            if (c >= '0' && c <= '9') return (byte)(c - '0');
            if (c >= 'a' && c <= 'f') return (byte)(c - 'a' + 10);
            if (c >= 'A' && c <= 'F') return (byte)(c - 'A' + 10);
            return 0;
        }
    }
}

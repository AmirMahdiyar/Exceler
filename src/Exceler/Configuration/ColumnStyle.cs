using Exceler.Core;
using System.Drawing;

namespace Exceler.Configuration
{
    /// <summary>
    /// Represents the visual formatting and styling options applied to an Excel worksheet column.
    /// Supports cross-platform color specifications (Hex codes, predefined ExcelColor, or System.Drawing.Color).
    /// </summary>
    public class ColumnStyle
    {
        /// <summary>
        /// Gets or sets whether text in this column should be rendered in bold font.
        /// </summary>
        public bool IsBold { get; set; } = false;

        /// <summary>
        /// Gets or sets the custom number or date format pattern (e.g., "$#,##0.00", "yyyy-mm-dd", "0.0%").
        /// </summary>
        public string? NumberFormat { get; set; }

        /// <summary>
        /// Gets or sets the explicit width of the column in characters.
        /// </summary>
        public double? Width { get; set; }

        /// <summary>
        /// Gets or sets the background fill color as a hex string (e.g., "#FF0000").
        /// Fully cross-platform compatible on Windows, Linux, and Docker.
        /// </summary>
        public string? BackgroundColorHex { get; set; }

        /// <summary>
        /// Gets or sets the text font color as a hex string (e.g., "#000000").
        /// Fully cross-platform compatible on Windows, Linux, and Docker.
        /// </summary>
        public string? FontColorHex { get; set; }

        /// <summary>
        /// Gets or sets the background fill color as a <see cref="Color"/> instance.
        /// Automatically synchronizes with <see cref="BackgroundColorHex"/>.
        /// </summary>
        public Color? BackgroundColor
        {
            get => !string.IsNullOrEmpty(BackgroundColorHex) ? ColorHelper.FromHex(BackgroundColorHex) : null;
            set => BackgroundColorHex = value.HasValue ? ColorHelper.ToHex(value.Value) : null;
        }

        /// <summary>
        /// Gets or sets the text font color as a <see cref="Color"/> instance.
        /// Automatically synchronizes with <see cref="FontColorHex"/>.
        /// </summary>
        public Color? FontColor
        {
            get => !string.IsNullOrEmpty(FontColorHex) ? ColorHelper.FromHex(FontColorHex) : null;
            set => FontColorHex = value.HasValue ? ColorHelper.ToHex(value.Value) : null;
        }
    }
}

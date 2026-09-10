using Exceler.Core;
using System.Drawing;

namespace Exceler.Configuration
{
    public class ColumnStyle
    {
        public bool IsBold { get; set; } = false;
        public string? NumberFormat { get; set; }

        /// <summary>
        /// Gets or sets the explicit width of the column in characters.
        /// </summary>
        public double? Width { get; set; }

        /// <summary>
        /// Gets or sets the background color as a hex string (e.g., "#FF0000").
        /// Cross-platform compatible on Windows, Linux, and Docker.
        /// </summary>
        public string? BackgroundColorHex { get; set; }

        /// <summary>
        /// Gets or sets the font color as a hex string (e.g., "#000000").
        /// Cross-platform compatible on Windows, Linux, and Docker.
        /// </summary>
        public string? FontColorHex { get; set; }

        public Color? BackgroundColor
        {
            get => !string.IsNullOrEmpty(BackgroundColorHex) ? ColorHelper.FromHex(BackgroundColorHex) : null;
            set => BackgroundColorHex = value.HasValue ? ColorHelper.ToHex(value.Value) : null;
        }

        public Color? FontColor
        {
            get => !string.IsNullOrEmpty(FontColorHex) ? ColorHelper.FromHex(FontColorHex) : null;
            set => FontColorHex = value.HasValue ? ColorHelper.ToHex(value.Value) : null;
        }
    }
}

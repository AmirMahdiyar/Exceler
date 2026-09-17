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

        /// <summary>
        /// Gets or sets the CLR type of the mapped property for this column.
        /// </summary>
        internal Type? PropertyType { get; set; }

        /// <summary>
        /// Gets or sets the optional dropdown list (Data Validation) configuration for this column.
        /// </summary>
        public DropdownConfiguration? Dropdown { get; set; }

        /// <summary>
        /// Gets the collection of conditional style rules configured for this column.
        /// Evaluated row-by-row during streaming; first matching rule takes precedence.
        /// </summary>
        public List<IConditionalStyleRule> ConditionalStyles { get; } = new();

        /// <summary>
        /// Creates a new <see cref="ColumnStyle"/> instance representing this style merged with an override style.
        /// Preserves base number formats and non-overridden properties.
        /// </summary>
        /// <param name="overrideStyle">The conditional or overriding style.</param>
        /// <returns>A merged <see cref="ColumnStyle"/> instance.</returns>
        public ColumnStyle MergeWith(ColumnStyle overrideStyle)
        {
            if (overrideStyle == null) return this;

            return new ColumnStyle
            {
                IsBold = overrideStyle.IsBold || this.IsBold,
                NumberFormat = overrideStyle.NumberFormat ?? this.NumberFormat,
                Width = overrideStyle.Width ?? this.Width,
                BackgroundColorHex = overrideStyle.BackgroundColorHex ?? this.BackgroundColorHex,
                FontColorHex = overrideStyle.FontColorHex ?? this.FontColorHex,
                Dropdown = overrideStyle.Dropdown ?? this.Dropdown,
                PropertyType = this.PropertyType
            };
        }

        /// <summary>
        /// Sets whether text should be rendered in bold font.
        /// </summary>
        /// <param name="isBold"><c>true</c> for bold text; otherwise <c>false</c>.</param>
        /// <returns>This <see cref="ColumnStyle"/> instance for fluent chaining.</returns>
        public ColumnStyle SetBold(bool isBold = true)
        {
            IsBold = isBold;
            return this;
        }

        /// <summary>
        /// Sets the number or date format pattern for this style (e.g., "$#,##0.00", "yyyy-mm-dd", "0.0%").
        /// </summary>
        /// <param name="format">The Excel format pattern string.</param>
        /// <returns>This <see cref="ColumnStyle"/> instance for fluent chaining.</returns>
        public ColumnStyle WithFormat(string format)
        {
            NumberFormat = format;
            return this;
        }

        /// <summary>
        /// Sets the number or date format pattern for this style (e.g., "$#,##0.00", "yyyy-mm-dd", "0.0%").
        /// </summary>
        /// <param name="format">The Excel format pattern string.</param>
        /// <returns>This <see cref="ColumnStyle"/> instance for fluent chaining.</returns>
        public ColumnStyle WithNumberFormat(string format)
        {
            return WithFormat(format);
        }

        /// <summary>
        /// Sets an explicit width for the column in characters.
        /// </summary>
        /// <param name="width">The width in characters.</param>
        /// <returns>This <see cref="ColumnStyle"/> instance for fluent chaining.</returns>
        public ColumnStyle WithWidth(double width)
        {
            Width = width;
            return this;
        }

        /// <summary>
        /// Sets the background fill color using a predefined <see cref="ExcelColor"/>.
        /// </summary>
        /// <param name="color">The predefined Excel color.</param>
        /// <returns>This <see cref="ColumnStyle"/> instance for fluent chaining.</returns>
        public ColumnStyle WithBackgroundColor(ExcelColor color)
        {
            BackgroundColorHex = ColorHelper.ToHex(color);
            return this;
        }

        /// <summary>
        /// Sets the background fill color using a hex color code (e.g., "#FF0000") or color name.
        /// </summary>
        /// <param name="color">The hex color code or name.</param>
        /// <returns>This <see cref="ColumnStyle"/> instance for fluent chaining.</returns>
        public ColumnStyle WithBackgroundColor(string color)
        {
            BackgroundColorHex = color;
            return this;
        }

        /// <summary>
        /// Sets the background fill color using a <see cref="Color"/> instance.
        /// </summary>
        /// <param name="color">The color instance.</param>
        /// <returns>This <see cref="ColumnStyle"/> instance for fluent chaining.</returns>
        public ColumnStyle WithBackgroundColor(Color color)
        {
            BackgroundColor = color;
            return this;
        }

        /// <summary>
        /// Sets the font color using a predefined <see cref="ExcelColor"/>.
        /// </summary>
        /// <param name="color">The predefined Excel color.</param>
        /// <returns>This <see cref="ColumnStyle"/> instance for fluent chaining.</returns>
        public ColumnStyle WithFontColor(ExcelColor color)
        {
            FontColorHex = ColorHelper.ToHex(color);
            return this;
        }

        /// <summary>
        /// Sets the font color using a hex color code (e.g., "#000000") or color name.
        /// </summary>
        /// <param name="color">The hex color code or name.</param>
        /// <returns>This <see cref="ColumnStyle"/> instance for fluent chaining.</returns>
        public ColumnStyle WithFontColor(string color)
        {
            FontColorHex = color;
            return this;
        }

        /// <summary>
        /// Sets the font color using a <see cref="Color"/> instance.
        /// </summary>
        /// <param name="color">The color instance.</param>
        /// <returns>This <see cref="ColumnStyle"/> instance for fluent chaining.</returns>
        public ColumnStyle WithFontColor(Color color)
        {
            FontColor = color;
            return this;
        }
    }
}

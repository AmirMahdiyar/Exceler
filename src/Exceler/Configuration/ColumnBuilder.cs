using Exceler.Abstractions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Configuration
{
    /// <summary>
    /// Provides a fluent API for configuring Excel column mappings.
    /// </summary>
    /// <typeparam name="TInput">The type of the model representing a single Excel row.</typeparam>
    /// <typeparam name="TProperty">The type of the property being mapped.</typeparam>
    public class ColumnBuilder<TInput, TProperty> : IColumnBuilder<TInput> where TInput : class
    {
        private readonly Expression<Func<TInput, TProperty>> _propertySelector;
        private int _columnIndex;
        private string? _headerName;

        private IExcelValueConverter<TProperty>? _converter;
        private readonly ColumnStyle _style = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnBuilder{TInput, TProperty}"/> class.
        /// </summary>
        /// <param name="propertySelector">An expression selecting the model property to be mapped.</param>
        internal ColumnBuilder(Expression<Func<TInput, TProperty>> propertySelector)
        {
            _propertySelector = propertySelector;
        }

        /// <summary>
        /// Maps the property to a specific 1-based column index in the Excel worksheet.
        /// </summary>
        /// <param name="columnIndex">The 1-based index of the column (e.g., 1 for column A, 2 for column B).</param>
        /// <returns>The current <see cref="ColumnBuilder{TInput, TProperty}"/> instance.</returns>
        public ColumnBuilder<TInput, TProperty> ToColumn(int columnIndex)
        {
            _columnIndex = columnIndex;
            return this;
        }

        /// <summary>
        /// Specifies the header name for the column, used primarily during Excel export operations.
        /// </summary>
        /// <param name="headerName">The exact string to display in the header row.</param>
        /// <returns>The current <see cref="ColumnBuilder{TInput, TProperty}"/> instance.</returns>
        public ColumnBuilder<TInput, TProperty> WithHeader(string headerName)
        {
            _headerName = headerName;
            return this;
        }

        /// <summary>
        /// Assigns a custom value converter to handle complex transformations for this specific column.
        /// </summary>
        /// <param name="converter">An instance of a class implementing <see cref="IExcelValueConverter{TProperty}"/>.</param>
        /// <returns>The current <see cref="ColumnBuilder{TInput, TProperty}"/> instance.</returns>
        public ColumnBuilder<TInput, TProperty> WithConverter(IExcelValueConverter<TProperty> converter)
        {
            _converter = converter;
            return this;
        }

        /// <summary>
        /// Sets whether text in this column (header and data) should be rendered in bold font.
        /// </summary>
        /// <param name="isBold">True to render in bold font; otherwise false. Default is true.</param>
        /// <returns>The current <see cref="ColumnBuilder{TInput, TProperty}"/> instance for fluent chaining.</returns>
        public ColumnBuilder<TInput, TProperty> IsBold(bool isBold = true)
        {
            _style.IsBold = isBold;
            return this;
        }

        /// <summary>
        /// Sets the number or date format pattern for this column (e.g., "$#,##0.00", "yyyy-mm-dd", "0.0%").
        /// </summary>
        /// <param name="format">The Excel format pattern string.</param>
        /// <returns>The current <see cref="ColumnBuilder{TInput, TProperty}"/> instance for fluent chaining.</returns>
        public ColumnBuilder<TInput, TProperty> WithFormat(string format)
        {
            _style.NumberFormat = format;
            return this;
        }

        /// <summary>
        /// Sets the number or date format pattern for the column (e.g., "$#,##0.00", "yyyy-mm-dd", "0.0%").
        /// </summary>
        /// <param name="format">The Excel format pattern string.</param>
        /// <returns>The current <see cref="ColumnBuilder{TInput, TProperty}"/> instance for fluent chaining.</returns>
        public ColumnBuilder<TInput, TProperty> WithNumberFormat(string format)
        {
            return WithFormat(format);
        }

        /// <summary>
        /// Sets an explicit width for the column in characters.
        /// Especially recommended when AutoFitColumns is disabled for high-volume exports.
        /// </summary>
        /// <param name="width">The column width in characters.</param>
        /// <returns>The current <see cref="ColumnBuilder{TInput, TProperty}"/> instance for fluent chaining.</returns>
        public ColumnBuilder<TInput, TProperty> WithWidth(double width)
        {
            _style.Width = width;
            return this;
        }

        /// <summary>
        /// Sets the background fill color of the column using a predefined <see cref="ExcelColor"/>.
        /// </summary>
        /// <param name="color">The predefined Excel color to apply.</param>
        /// <returns>The current <see cref="ColumnBuilder{TInput, TProperty}"/> instance for fluent chaining.</returns>
        public ColumnBuilder<TInput, TProperty> WithBackgroundColor(ExcelColor color)
        {
            _style.BackgroundColorHex = Exceler.Core.ColorHelper.ToHex(color);
            return this;
        }

        /// <summary>
        /// Sets the background fill color of the column using a hex color string (e.g., "#FF0000") or color name (e.g., "blue").
        /// Fully cross-platform compatible with Windows, Linux, and Docker.
        /// </summary>
        /// <param name="color">The hex color code (e.g. "#RRGGBB") or color name.</param>
        /// <returns>The current <see cref="ColumnBuilder{TInput, TProperty}"/> instance for fluent chaining.</returns>
        public ColumnBuilder<TInput, TProperty> WithBackgroundColor(string color)
        {
            _style.BackgroundColorHex = color;
            return this;
        }

        /// <summary>
        /// Sets the background fill color of the column using a <see cref="Color"/> instance.
        /// </summary>
        /// <param name="color">The system drawing color to apply.</param>
        /// <returns>The current <see cref="ColumnBuilder{TInput, TProperty}"/> instance for fluent chaining.</returns>
        public ColumnBuilder<TInput, TProperty> WithBackgroundColor(Color color)
        {
            _style.BackgroundColor = color;
            return this;
        }

        /// <summary>
        /// Sets the font color of the column using a predefined <see cref="ExcelColor"/>.
        /// </summary>
        /// <param name="color">The predefined Excel color to apply to text.</param>
        /// <returns>The current <see cref="ColumnBuilder{TInput, TProperty}"/> instance for fluent chaining.</returns>
        public ColumnBuilder<TInput, TProperty> WithFontColor(ExcelColor color)
        {
            _style.FontColorHex = Exceler.Core.ColorHelper.ToHex(color);
            return this;
        }

        /// <summary>
        /// Sets the font color of the column using a hex color string (e.g., "#000000") or color name (e.g., "white").
        /// Fully cross-platform compatible with Windows, Linux, and Docker.
        /// </summary>
        /// <param name="color">The hex color code (e.g. "#RRGGBB") or color name.</param>
        /// <returns>The current <see cref="ColumnBuilder{TInput, TProperty}"/> instance for fluent chaining.</returns>
        public ColumnBuilder<TInput, TProperty> WithFontColor(string color)
        {
            _style.FontColorHex = color;
            return this;
        }

        /// <summary>
        /// Sets the font color of the column using a <see cref="Color"/> instance.
        /// </summary>
        /// <param name="color">The system drawing color to apply to text.</param>
        /// <returns>The current <see cref="ColumnBuilder{TInput, TProperty}"/> instance for fluent chaining.</returns>
        public ColumnBuilder<TInput, TProperty> WithFontColor(Color color)
        {
            _style.FontColor = color;
            return this;
        }

        /// <inheritdoc />
        void IColumnBuilder<TInput>.Compile(ExcelProfile<TInput> profile)
        {
            profile.RegisterMapping(_propertySelector, _columnIndex, _headerName, _converter, _style);
        }
    }
}
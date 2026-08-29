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
    public class ColumnBuilder<TInput, TProperty> : IColumnBuilder<TInput> where TInput : class, new()
    {
        private readonly Expression<Func<TInput, TProperty>> _propertySelector;
        private int _columnIndex;
        private string? _headerName;

        private IExcelValueConverter<TProperty>? _converter;
        private readonly ColumnStyle _style = new();

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

        public ColumnBuilder<TInput, TProperty> IsBold(bool isBold = true)
        {
            _style.IsBold = isBold;
            return this;
        }

        public ColumnBuilder<TInput, TProperty> WithFormat(string format)
        {
            _style.NumberFormat = format;
            return this;
        }

        public ColumnBuilder<TInput, TProperty> WithBackgroundColor(Color color)
        {
            _style.BackgroundColor = color;
            return this;
        }

        public ColumnBuilder<TInput, TProperty> WithFontColor(Color color)
        {
            _style.FontColor = color;
            return this;
        }

        void IColumnBuilder<TInput>.Compile(ExcelProfile<TInput> profile)
        {
            profile.RegisterMapping(_propertySelector, _columnIndex, _headerName, _converter, _style);
        }
    }
}
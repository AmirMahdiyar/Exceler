using Exceler.Abstractions;
using Exceler.Core.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Configuration
{
    /// <summary>
    /// Defines the mapping profile for an Excel input model. 
    /// Inherit from this class to configure column indices, headers, and custom converters using the Fluent API.
    /// </summary>
    /// <typeparam name="TInput">The type of the model representing a single Excel row.</typeparam>
    public abstract class ExcelProfile<TInput> where TInput : class
    {
        /// <summary>
        /// Gets the dictionary of compiled property setter delegates indexed by 1-based column number.
        /// </summary>
        internal Dictionary<int, Action<TInput, object>> CompiledSetters { get; } = new();

        /// <summary>
        /// Gets the dictionary of compiled property getter delegates indexed by 1-based column number.
        /// </summary>
        internal Dictionary<int, Func<TInput, object>> CompiledGetters { get; } = new();

        /// <summary>
        /// Gets the dictionary of column header names indexed by 1-based column number.
        /// </summary>
        internal Dictionary<int, string> ColumnHeaders { get; } = new();

        /// <summary>
        /// Gets the dictionary of column style configurations indexed by 1-based column number.
        /// </summary>
        internal Dictionary<int, ColumnStyle> ColumnStyles { get; } = new();

        /// <summary>
        /// Gets the list of registered column builders before compilation.
        /// </summary>
        internal List<IColumnBuilder<TInput>> Builders { get; } = new();
        private bool _isBuilt = false;
        /// <summary>
        /// Gets whether string values read from Excel cells should be automatically trimmed of leading and trailing whitespace.
        /// Default is true.
        /// </summary>
        public bool TrimStringValues { get; protected set; } = true;

        /// <summary>
        /// Gets whether header template validation should be executed during reading to prevent schema mismatches.
        /// Default is true.
        /// </summary>
        public bool ValidateTemplateOnRead { get; protected set; } = true;

        /// <summary>
        /// Gets or sets whether the worksheet view orientation should be Right-To-Left (RTL).
        /// Default is false (Left-To-Right).
        /// </summary>
        public bool RightToLeft { get; protected set; } = false;

        /// <summary>
        /// Gets or sets whether column widths should be automatically calculated based on content.
        /// Default is true. Disabling this is recommended for high-volume exports to avoid performance bottlenecks.
        /// </summary>
        public bool AutoFitColumns { get; protected set; } = true;

        /// <summary>
        /// Configures the worksheet view orientation to Right-To-Left (RTL).
        /// </summary>
        /// <param name="enabled">True to enable Right-To-Left; otherwise false.</param>
        /// <returns>The current profile instance for fluent chaining.</returns>
        protected ExcelProfile<TInput> WithRightToLeft(bool enabled = true)
        {
            RightToLeft = enabled;
            return this;
        }

        /// <summary>
        /// Configures whether columns should be automatically fitted to their content.
        /// </summary>
        /// <param name="enabled">True to enable AutoFitColumns; otherwise false.</param>
        /// <returns>The current profile instance for fluent chaining.</returns>
        protected ExcelProfile<TInput> WithAutoFitColumns(bool enabled = true)
        {
            AutoFitColumns = enabled;
            return this;
        }

        /// <summary>
        /// Configures whether string values read from cells should be trimmed.
        /// </summary>
        /// <param name="enabled">True to enable trimming of strings; otherwise false.</param>
        /// <returns>The current profile instance for fluent chaining.</returns>
        protected ExcelProfile<TInput> WithTrimStringValues(bool enabled = true)
        {
            TrimStringValues = enabled;
            return this;
        }

        /// <summary>
        /// Configures whether header template validation should be executed during reading.
        /// </summary>
        /// <param name="enabled">True to enable header template validation; otherwise false.</param>
        /// <returns>The current profile instance for fluent chaining.</returns>
        protected ExcelProfile<TInput> WithValidateTemplateOnRead(bool enabled = true)
        {
            ValidateTemplateOnRead = enabled;
            return this;
        }

        /// <summary>
        /// Initiates the mapping configuration for a specific property of the input model.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property being mapped.</typeparam>
        /// <param name="propertySelector">An expression specifying the property to map (e.g., x => x.Name).</param>
        /// <returns>A <see cref="ColumnBuilder{TInput, TProperty}"/> to chain mapping configurations.</returns>
        protected ColumnBuilder<TInput, TProperty> Map<TProperty>(Expression<Func<TInput, TProperty>> propertySelector)
        {
            var builder = new ColumnBuilder<TInput, TProperty>(propertySelector);
            Builders.Add(builder);
            return builder;
        }

        /// <summary>
        /// Compiles the mapping configurations into high-performance Expression Trees.
        /// This method is called automatically by the framework before the first read or write operation.
        /// </summary>
        internal void EnsureBuilt()
        {
            if (_isBuilt) return;
            lock (this)
            {
                if (_isBuilt) return;
                foreach (var builder in Builders)
                {
                    builder.Compile(this);
                }
                _isBuilt = true;
            }
        }

        /// <summary>
        /// Registers a column header title for the specified column index.
        /// </summary>
        /// <param name="columnIndex">The 1-based column index.</param>
        /// <param name="headerName">The header text to display.</param>
        internal void RegisterHeader(int columnIndex, string headerName)
        {
            ColumnHeaders[columnIndex] = headerName;
        }

        /// <summary>
        /// Registers a complete column mapping including property selector, converter, and style configuration.
        /// </summary>
        /// <typeparam name="TProperty">The type of the mapped property.</typeparam>
        /// <param name="propertySelector">The expression selecting the property.</param>
        /// <param name="columnIndex">The 1-based column index.</param>
        /// <param name="headerName">The optional header title.</param>
        /// <param name="converter">The optional custom value converter.</param>
        /// <param name="style">The column style configuration.</param>
        internal void RegisterMapping<TProperty>(
                Expression<Func<TInput, TProperty>> propertySelector,
                int columnIndex,
                string? headerName,
                IExcelValueConverter<TProperty>? converter,
                ColumnStyle style)
        {
            if (!string.IsNullOrEmpty(headerName))
                RegisterHeader(columnIndex, headerName);

            ColumnStyles[columnIndex] = style;

            var (setter, getter) = MappingExpressionBuilder.BuildDelegates(propertySelector, converter);

            if (setter != null)
                CompiledSetters[columnIndex] = setter;

            CompiledGetters[columnIndex] = getter;
        }
    }
}

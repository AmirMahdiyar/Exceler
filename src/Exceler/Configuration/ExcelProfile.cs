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
    public abstract class ExcelProfile<TInput> where TInput : class, new()
    {
        internal Dictionary<int, Action<TInput, object>> CompiledSetters { get; } = new();
        internal Dictionary<int, Func<TInput, object>> CompiledGetters { get; } = new();
        internal Dictionary<int, string> ColumnHeaders { get; } = new();
        internal Dictionary<int, ColumnStyle> ColumnStyles { get; } = new();
        internal List<IColumnBuilder<TInput>> Builders { get; } = new();
        private bool _isBuilt = false;
        public bool TrimStringValues { get; protected set; } = true;
        public bool ValidateTemplateOnRead { get; protected set; } = true;

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

        internal void RegisterHeader(int columnIndex, string headerName)
        {
            ColumnHeaders[columnIndex] = headerName;
        }

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

            CompiledSetters[columnIndex] = setter;
            CompiledGetters[columnIndex] = getter;
        }
    }
}

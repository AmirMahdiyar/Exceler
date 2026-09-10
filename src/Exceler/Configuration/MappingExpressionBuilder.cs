using Exceler.Abstractions;
using Exceler.Core.Converter;
using System.Linq.Expressions;
using System.Reflection;

namespace Exceler.Configuration
{
    /// <summary>
    /// Compiles LINQ expression trees into high-performance, strongly-typed getter and setter delegates for model mapping.
    /// </summary>
    internal static class MappingExpressionBuilder
    {
        /// <summary>
        /// Builds compiled getter and optional setter delegates for the specified property expression and converter.
        /// </summary>
        /// <typeparam name="TInput">The type of the input model.</typeparam>
        /// <typeparam name="TProperty">The type of the property being mapped.</typeparam>
        /// <param name="propertySelector">An expression selecting the model property (e.g., x => x.Name).</param>
        /// <param name="converter">An optional custom value converter for cell transformations.</param>
        /// <returns>A tuple containing the compiled setter action (or null if property is read-only) and the compiled getter function.</returns>
        public static (Action<TInput, object>? Setter, Func<TInput, object> Getter) BuildDelegates<TInput, TProperty>(
            Expression<Func<TInput, TProperty>> propertySelector,
            IExcelValueConverter<TProperty>? converter)
            where TInput : class
        {
            var propertyInfo = GetPropertyInfo(propertySelector);
            var instanceParam = Expression.Parameter(typeof(TInput), "instance");
            var propertyAccess = Expression.Property(instanceParam, propertyInfo);

            Action<TInput, object>? setter = null;
            if (propertyInfo.CanWrite)
            {
                try
                {
                    var valueParam = Expression.Parameter(typeof(object), "value");
                    var valueToAssign = BuildAssignExpression(propertyInfo, valueParam, converter);
                    var assign = Expression.Assign(propertyAccess, valueToAssign);
                    setter = Expression.Lambda<Action<TInput, object>>(assign, instanceParam, valueParam).Compile();
                }
                catch
                {
                    // Init-only or custom accessor properties may not support Expression.Assign.
                    // Setter is only required when reading from Excel, not when writing.
                    setter = null;
                }
            }

            var valueToExport = BuildExportExpression(propertyAccess, converter);
            var getter = Expression.Lambda<Func<TInput, object>>(valueToExport, instanceParam).Compile();

            return (setter, getter);
        }

        /// <summary>
        /// Extracts the <see cref="PropertyInfo"/> from a member access lambda expression.
        /// </summary>
        /// <typeparam name="TInput">The type containing the property.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertySelector">The expression selecting the property.</param>
        /// <returns>The extracted <see cref="PropertyInfo"/>.</returns>
        private static PropertyInfo GetPropertyInfo<TInput, TProperty>(Expression<Func<TInput, TProperty>> propertySelector)
        {
            var memberExpression = propertySelector.Body as MemberExpression ?? (propertySelector.Body as UnaryExpression)?.Operand as MemberExpression;
            return (PropertyInfo)memberExpression!.Member;
        }

        /// <summary>
        /// Builds the assignment expression to convert an incoming raw Excel cell value into the target property type.
        /// </summary>
        /// <typeparam name="TProperty">The target property type.</typeparam>
        /// <param name="propertyInfo">The property metadata.</param>
        /// <param name="valueParam">The expression representing the input cell value.</param>
        /// <param name="converter">The optional custom converter.</param>
        /// <returns>The compiled expression that evaluates the converted value.</returns>
        private static Expression BuildAssignExpression<TProperty>(PropertyInfo propertyInfo, ParameterExpression valueParam, IExcelValueConverter<TProperty>? converter)
        {
            if (converter != null)
            {
                var converterConstant = Expression.Constant(converter, typeof(IExcelValueConverter<TProperty>));
                var method = typeof(IExcelValueConverter<TProperty>).GetMethod(nameof(IExcelValueConverter<TProperty>.ConvertFromExcel))!;
                return Expression.Call(converterConstant, method, valueParam);
            }

            var changeTypeMethod = typeof(SafeConverter)
                    .GetMethod(nameof(SafeConverter.ChangeType), BindingFlags.Public | BindingFlags.Static)!
                    .MakeGenericMethod(propertyInfo.PropertyType);

            return Expression.Call(changeTypeMethod, valueParam);
        }

        /// <summary>
        /// Builds the export expression to retrieve and format the property value for writing into an Excel cell.
        /// </summary>
        /// <typeparam name="TProperty">The source property type.</typeparam>
        /// <param name="propertyAccess">The expression accessing the model property.</param>
        /// <param name="converter">The optional custom converter.</param>
        /// <returns>The expression that produces the value for the Excel cell.</returns>
        private static Expression BuildExportExpression<TProperty>(MemberExpression propertyAccess, IExcelValueConverter<TProperty>? converter)
        {
            if (converter != null)
            {
                var converterConstant = Expression.Constant(converter, typeof(IExcelValueConverter<TProperty>));
                var method = typeof(IExcelValueConverter<TProperty>).GetMethod(nameof(IExcelValueConverter<TProperty>.ConvertToExcel))!;
                return Expression.Call(converterConstant, method, propertyAccess);
            }

            return Expression.Convert(propertyAccess, typeof(object));
        }
    }
}

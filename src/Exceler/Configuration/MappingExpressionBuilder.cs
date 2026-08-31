using Exceler.Abstractions;
using Exceler.Core.Converter;
using System.Linq.Expressions;
using System.Reflection;

namespace Exceler.Configuration
{
    internal static class MappingExpressionBuilder
    {
        public static (Action<TInput, object> Setter, Func<TInput, object> Getter) BuildDelegates<TInput, TProperty>(
            Expression<Func<TInput, TProperty>> propertySelector,
            IExcelValueConverter<TProperty>? converter)
            where TInput : class
        {
            var propertyInfo = GetPropertyInfo(propertySelector);
            var instanceParam = Expression.Parameter(typeof(TInput), "instance");
            var valueParam = Expression.Parameter(typeof(object), "value");
            var propertyAccess = Expression.Property(instanceParam, propertyInfo);

            var valueToAssign = BuildAssignExpression(propertyInfo, valueParam, converter);
            var valueToExport = BuildExportExpression(propertyAccess, converter);

            var assign = Expression.Assign(propertyAccess, valueToAssign);

            var setter = Expression.Lambda<Action<TInput, object>>(assign, instanceParam, valueParam).Compile();
            var getter = Expression.Lambda<Func<TInput, object>>(valueToExport, instanceParam).Compile();

            return (setter, getter);
        }

        private static PropertyInfo GetPropertyInfo<TInput, TProperty>(Expression<Func<TInput, TProperty>> propertySelector)
        {
            var memberExpression = propertySelector.Body as MemberExpression ?? (propertySelector.Body as UnaryExpression)?.Operand as MemberExpression;
            return (PropertyInfo)memberExpression!.Member;
        }

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

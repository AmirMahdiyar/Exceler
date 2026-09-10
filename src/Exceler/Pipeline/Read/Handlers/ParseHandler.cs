using Exceler.Core.Exceptions;
using System;
using System.Linq;
using System.Reflection;

namespace Exceler.Pipeline.Read.Handlers
{
    /// <summary>
    /// Responsible for extracting raw values from Excel cells, sanitizing them, and mapping them to the input model properties.
    /// </summary>
    internal class ParseHandler<TInput, TOutput> : ReadHandler<TInput, TOutput> where TInput : class, new()
    {
        /// <inheritdoc />
        public override void Handle(ReadContext<TInput, TOutput> context)
        {
            var activeSetters = context.ActiveSetters ?? context.Profile.CompiledSetters.Where(s => s.Key <= context.ColCount).ToArray();
            var rowValues = context.RowValues;

            for (int i = 0; i < activeSetters.Length; i++)
            {
                var setter = activeSetters[i];
                try
                {
                    var cellValue = rowValues != null && i < rowValues.Length
                        ? rowValues[i]
                        : context.Worksheet.Cells[context.Row, setter.Key].Value;

                    if (context.Profile.TrimStringValues && cellValue is string strValue)
                        cellValue = string.IsNullOrWhiteSpace(strValue) ? null : strValue.Trim();

                    setter.Value(context.InputModel, cellValue!);
                }
                catch (ExcelCastException)
                {
                    var colName = GetColumnName(context, setter.Key);
                    context.Result.Errors.Add($"Format of [{colName}] Column is incorrect");
                }
                catch (Exception ex)
                {
                    var actualException = ex is TargetInvocationException tie && tie.InnerException != null
                        ? tie.InnerException
                        : ex;

                    var colName = GetColumnName(context, setter.Key);

                    if (actualException is ExcelCastException)
                    {
                        context.Result.Errors.Add($"Format of [{colName}] Column is incorrect");
                    }
                    else
                    {
                        var message = !string.IsNullOrWhiteSpace(actualException.Message)
                            ? actualException.Message
                            : actualException.GetType().Name;

                        context.Result.Errors.Add($"Error parsing [{colName}] Column: {message}");
                    }
                }
            }

            if (context.Result.IsValid && Next != null)
                Next.Handle(context);
        }

        /// <inheritdoc />
        public override async Task HandleAsync(ReadContext<TInput, TOutput> context, CancellationToken cancellationToken = default)
        {
            Handle(context);

            if (context.Result.IsValid && Next != null)
                await Next.HandleAsync(context, cancellationToken);
        }

        /// <summary>
        /// Retrieves the display column name from the profile headers, or a default 1-based column label if unnamed.
        /// </summary>
        /// <param name="context">The read context.</param>
        /// <param name="columnIndex">The 1-based column index.</param>
        /// <returns>The resolved column title.</returns>
        private static string GetColumnName(ReadContext<TInput, TOutput> context, int columnIndex)
        {
            return context.Profile.ColumnHeaders.TryGetValue(columnIndex, out var headerName)
                ? headerName
                : $"Column {columnIndex}";
        }
    }
}

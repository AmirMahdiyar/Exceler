using Exceler.Core.Exceptions;
using Exceler.Pipeline.Read;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Pipeline.Read.Handlers
{
    /// <summary>
    /// Responsible for extracting raw values from Excel cells, sanitizing them, and mapping them to the input model properties.
    /// </summary>
    internal class ParseHandler<TInput, TOutput> : ReadHandler<TInput, TOutput> where TInput : class, new()
    {
        public override void Handle(ReadContext<TInput, TOutput> context)
        {
            foreach (var setter in context.Profile.CompiledSetters.Where(s => s.Key <= context.ColCount))
            {
                var cellValue = context.Worksheet.Cells[context.Row, setter.Key].Value;

                if (context.Profile.TrimStringValues && cellValue is string strValue)
                    cellValue = string.IsNullOrWhiteSpace(strValue) ? null : strValue.Trim();

                try
                {
                    setter.Value(context.InputModel, cellValue);
                }
                catch (ExcelCastException)
                {
                    context.Profile.ColumnHeaders.TryGetValue(setter.Key, out var headerName);
                    var colName = headerName ?? $"Column {setter.Key}";
                    context.Result.Errors.Add($"Format of [{colName}] Column is incorrect.");
                }
            }

            if (context.Result.IsValid && Next != null)
                Next.Handle(context);
        }
    }
}

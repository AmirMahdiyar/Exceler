namespace Exceler.Pipeline.Write.Handlers
{
    /// <summary>
    /// Responsible for iterating through the provided data collection, evaluating the compiled getters, and populating the worksheet cells.
    /// </summary>
    internal class DataWriterHandler<TModel> : WriteHandler<TModel> where TModel : class
    {
        public override async Task HandleAsync(WriteContext<TModel> context)
        {

            int currentRow = context.Profile.ColumnHeaders.Any() ? 2 : 1;

            var gettersArray = context.Profile.CompiledGetters.ToArray();

            if (context.AsyncData != null)
                await foreach (var item in context.AsyncData)
                {
                    if (item == null) continue;

                    WriteRow(context.Worksheet, currentRow, item, gettersArray);
                    currentRow++;
                }
            else if (context.Data != null)
                foreach (var item in context.Data)
                {
                    if (item == null) continue;

                    WriteRow(context.Worksheet, currentRow, item, gettersArray);
                    currentRow++;
                }

            context.TotalRows = currentRow - 1;

            if (Next is not null)
                await Next.HandleAsync(context);
        }


        private void WriteRow(
            OfficeOpenXml.ExcelWorksheet worksheet,
            int row,
            TModel item,
            KeyValuePair<int, System.Func<TModel, object>>[] getters)
        {
            foreach (var kvp in getters)
            {
                var columnIndex = kvp.Key;
                var getter = kvp.Value;

                var finalValue = getter(item);

                if (finalValue is DateOnly dateOnly)
                {
                    worksheet.Cells[row, columnIndex].Value = dateOnly.ToDateTime(TimeOnly.MinValue);
                    if (string.IsNullOrEmpty(worksheet.Cells[row, columnIndex].Style.Numberformat.Format))
                    {
                        worksheet.Cells[row, columnIndex].Style.Numberformat.Format = "yyyy-mm-dd";
                    }
                }
                else if (finalValue is TimeOnly timeOnly)
                {
                    worksheet.Cells[row, columnIndex].Value = timeOnly.ToTimeSpan();
                    if (string.IsNullOrEmpty(worksheet.Cells[row, columnIndex].Style.Numberformat.Format))
                    {
                        worksheet.Cells[row, columnIndex].Style.Numberformat.Format = "hh:mm:ss";
                    }
                }
                else
                {
                    worksheet.Cells[row, columnIndex].Value = finalValue;
                }
            }
        }
    }
}

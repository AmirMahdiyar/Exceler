namespace Exceler.Pipeline.Write.Handlers
{
    /// <summary>
    /// Responsible for applying user-defined styles (colors, bold fonts, number formats) to the specifically populated cell ranges.
    /// </summary>
    internal class StyleWriterHandler<TModel> : WriteHandler<TModel> where TModel : class
    {
        public override async Task HandleAsync(WriteContext<TModel> context)
        {
            foreach (var kvp in context.Profile.ColumnStyles)
            {
                if (kvp.Value.Width.HasValue)
                {
                    context.Worksheet.Column(kvp.Key).Width = kvp.Value.Width.Value;
                }
            }

            int startRow = context.Profile.ColumnHeaders.Any() ? 2 : 1;

            if (context.TotalRows >= startRow)
            {
                foreach (var kvp in context.Profile.ColumnStyles)
                {
                    int colIndex = kvp.Key;
                    var style = kvp.Value;

                    var range = context.Worksheet.Cells[startRow, colIndex, context.TotalRows, colIndex];

                    if (style.IsBold)
                        range.Style.Font.Bold = true;

                    if (!string.IsNullOrEmpty(style.NumberFormat))
                        range.Style.Numberformat.Format = style.NumberFormat;

                    if (!string.IsNullOrEmpty(style.BackgroundColorHex))
                    {
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Exceler.Core.ColorHelper.FromHex(style.BackgroundColorHex));
                    }

                    if (!string.IsNullOrEmpty(style.FontColorHex))
                    {
                        range.Style.Font.Color.SetColor(Exceler.Core.ColorHelper.FromHex(style.FontColorHex));
                    }
                }
            }

            if (Next is not null)
                await Next.HandleAsync(context);
        }
    }
}

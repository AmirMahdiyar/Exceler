using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Pipeline.Write.Handlers
{
    /// <summary>
    /// Responsible for applying user-defined styles (colors, bold fonts, number formats) to the specifically populated cell ranges.
    /// </summary>
    internal class StyleWriterHandler<TModel> : WriteHandler<TModel> where TModel : class, new()
    {
        public override void Handle(WriteContext<TModel> context)
        {
            foreach (var kvp in context.Profile.ColumnStyles)
            {
                int colIndex = kvp.Key;
                var style = kvp.Value;

                var range = context.Worksheet.Cells[1, colIndex, context.TotalRows, colIndex];

                if (style.IsBold)
                    range.Style.Font.Bold = true;

                if (!string.IsNullOrEmpty(style.NumberFormat))
                    range.Style.Numberformat.Format = style.NumberFormat;

                if (style.BackgroundColor.HasValue)
                {
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(style.BackgroundColor.GetValueOrDefault());
                }

                if (style.FontColor.HasValue)
                {
                    range.Style.Font.Color.SetColor(style.FontColor.GetValueOrDefault());
                }
            }

            Next?.Handle(context);
        }
    }
}

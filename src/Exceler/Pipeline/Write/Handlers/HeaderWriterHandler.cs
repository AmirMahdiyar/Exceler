using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Pipeline.Write.Handlers
{
    /// <summary>
    /// Responsible for writing the mapped column headers to the first row of the worksheet and applying default header styling.
    /// </summary>
    internal class HeaderWriterHandler<TModel> : WriteHandler<TModel> where TModel : class, new()
    {
        public override void Handle(WriteContext<TModel> context)
        {
            foreach (var header in context.Profile.ColumnHeaders)
            {
                context.Worksheet.Cells[1, header.Key].Value = header.Value;
                context.Worksheet.Cells[1, header.Key].Style.Font.Bold = true;
            }

            Next?.Handle(context);
        }
    }
}

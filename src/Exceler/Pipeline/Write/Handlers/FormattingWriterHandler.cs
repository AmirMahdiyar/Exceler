using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Pipeline.Write.Handlers
{
    /// <summary>
    /// Responsible for applying final worksheet-level configurations, such as Right-To-Left view orientation and column auto-fitting.
    /// </summary>
    internal class FormattingWriterHandler<TModel> : WriteHandler<TModel> where TModel : class, new()
    {
        public override void Handle(WriteContext<TModel> context)
        {
            context.Worksheet.View.RightToLeft = false;
            context.Worksheet.Cells[context.Worksheet.Dimension.Address].AutoFitColumns();

            Next?.Handle(context);
        }
    }
}

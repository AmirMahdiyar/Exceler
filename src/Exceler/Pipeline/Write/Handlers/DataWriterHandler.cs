using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Pipeline.Write.Handlers
{
    /// <summary>
    /// Responsible for iterating through the provided data collection, evaluating the compiled getters, and populating the worksheet cells.
    /// </summary>
    internal class DataWriterHandler<TModel> : WriteHandler<TModel> where TModel : class, new()
    {
        public override void Handle(WriteContext<TModel> context)
        {
            int currentRow = 2;
            foreach (var item in context.Data)
            {
                foreach (var getter in context.Profile.CompiledGetters)
                {
                    context.Worksheet.Cells[currentRow, getter.Key].Value = getter.Value(item);
                }
                currentRow++;
            }
            // Record the total populated rows so styling handlers know the exact range bounds.
            context.TotalRows = currentRow - 1;

            Next?.Handle(context);
        }
    }
}

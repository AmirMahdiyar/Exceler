using Exceler.Pipeline.Read;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Pipeline.Read.Handlers
{
    /// <summary>
    /// Responsible for transforming the successfully parsed and validated input model into the final output representation.
    /// </summary>
    internal class ProcessHandler<TInput, TOutput> : ReadHandler<TInput, TOutput> where TInput : class, new()
    {
        public override void Handle(ReadContext<TInput, TOutput> context)
        {
            context.Result.Data = context.Processor.Process(context.InputModel);
        }
    }
}

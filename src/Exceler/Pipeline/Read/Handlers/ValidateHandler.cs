using Exceler.Pipeline.Read;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Pipeline.Read.Handlers
{
    /// <summary>
    /// Responsible for validating the fully populated input model against business rules using the configured <see cref="IExcelValidator{TInput}"/>.
    /// </summary>
    internal class ValidateHandler<TInput, TOutput> : ReadHandler<TInput, TOutput> where TInput : class, new()
    {
        public override void Handle(ReadContext<TInput, TOutput> context)
        {
            if (context.Validator != null)
            {
                var validationErrors = context.Validator.Validate(context.InputModel);
                if (validationErrors != null && validationErrors.Any())
                {
                    context.Result.Errors.AddRange(validationErrors);
                }
            }

            if (context.Result.IsValid && Next != null)
                Next.Handle(context);
        }
    }
}

using Exceler.Abstractions;
using Exceler.Pipeline.Read;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Exceler.Pipeline.Read.Handlers
{
    /// <summary>
    /// Responsible for validating the fully populated input model against business rules using the configured <see cref="IExcelValidator{TInput}"/> or <see cref="IAsyncExcelValidator{TInput}"/>.
    /// </summary>
    internal class ValidateHandler<TInput, TOutput> : ReadHandler<TInput, TOutput> where TInput : class, new()
    {
        /// <inheritdoc />
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
            else if (context.AsyncValidator != null)
            {
                var validationErrors = Task.Run(async () => await context.AsyncValidator.ValidateAsync(context.InputModel)).GetAwaiter().GetResult();
                if (validationErrors != null && validationErrors.Any())
                {
                    context.Result.Errors.AddRange(validationErrors);
                }
            }

            if (context.Result.IsValid && Next != null)
                Next.Handle(context);
        }

        /// <inheritdoc />
        public override async Task HandleAsync(ReadContext<TInput, TOutput> context, CancellationToken cancellationToken = default)
        {
            if (context.AsyncValidator != null)
            {
                var validationErrors = await context.AsyncValidator.ValidateAsync(context.InputModel, cancellationToken);
                if (validationErrors != null && validationErrors.Any())
                {
                    context.Result.Errors.AddRange(validationErrors);
                }
            }
            else if (context.Validator != null)
            {
                var validationErrors = context.Validator.Validate(context.InputModel);
                if (validationErrors != null && validationErrors.Any())
                {
                    context.Result.Errors.AddRange(validationErrors);
                }
            }

            if (context.Result.IsValid && Next != null)
                await Next.HandleAsync(context, cancellationToken);
        }
    }
}

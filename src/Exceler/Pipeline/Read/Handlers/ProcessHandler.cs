using Exceler.Pipeline.Read;
using System;
using System.Threading;
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
            try
            {
                if (context.Processor != null)
                {
                    context.Result.Data = context.Processor.Process(context.InputModel);
                }
                else if (context.AsyncProcessor != null)
                {
                    context.Result.Data = Task.Run(async () => await context.AsyncProcessor.ProcessAsync(context.InputModel)).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                var message = !string.IsNullOrWhiteSpace(ex.Message) ? ex.Message : ex.GetType().Name;
                context.Result.Errors.Add($"Processing error: {message}");
            }
        }

        public override async Task HandleAsync(ReadContext<TInput, TOutput> context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (context.AsyncProcessor != null)
                {
                    context.Result.Data = await context.AsyncProcessor.ProcessAsync(context.InputModel, cancellationToken);
                }
                else if (context.Processor != null)
                {
                    context.Result.Data = context.Processor.Process(context.InputModel);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                var message = !string.IsNullOrWhiteSpace(ex.Message) ? ex.Message : ex.GetType().Name;
                context.Result.Errors.Add($"Processing error: {message}");
            }
        }
    }
}

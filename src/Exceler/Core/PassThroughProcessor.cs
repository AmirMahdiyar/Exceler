using Exceler.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Exceler.Core
{
    /// <summary>
    /// Default pass-through processor that returns the input model as the output model when no custom processor is configured.
    /// </summary>
    /// <typeparam name="TInput">The input model type.</typeparam>
    /// <typeparam name="TOutput">The output model type.</typeparam>
    public class PassThroughProcessor<TInput, TOutput> : IExcelProcessor<TInput, TOutput>, IAsyncExcelProcessor<TInput, TOutput>
    {
        /// <summary>
        /// Transforms the input model by casting it directly to the output model type.
        /// </summary>
        /// <param name="input">The populated input model.</param>
        /// <returns>The same instance cast to <typeparamref name="TOutput"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown when <typeparamref name="TInput"/> cannot be converted to <typeparamref name="TOutput"/>.</exception>
        public TOutput Process(TInput input)
        {
            if (input is TOutput output)
                return output;

            throw new InvalidOperationException(
                $"Cannot pass through model of type '{typeof(TInput).FullName}' to '{typeof(TOutput).FullName}'. " +
                $"When input and output types differ, please register a custom IExcelProcessor<{typeof(TInput).Name}, {typeof(TOutput).Name}> in the service collection.");
        }

        /// <summary>
        /// Asynchronously transforms the input model by casting it directly to the output model type.
        /// </summary>
        /// <param name="input">The populated input model.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A completed task containing the input cast to <typeparamref name="TOutput"/>.</returns>
        public Task<TOutput> ProcessAsync(TInput input, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Process(input));
        }
    }

    /// <summary>
    /// Default pass-through processor for scenarios where input and output models are of the same type.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public class PassThroughProcessor<T> : PassThroughProcessor<T, T>
    {
    }
}

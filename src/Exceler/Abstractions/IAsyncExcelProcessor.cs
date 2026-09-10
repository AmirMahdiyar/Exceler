using System.Threading;
using System.Threading.Tasks;

namespace Exceler.Abstractions
{
    /// <summary>
    /// Defines an asynchronous processor that transforms the raw mapped Excel input model into a final Data Transfer Object (DTO) or domain entity.
    /// </summary>
    /// <typeparam name="TInput">The raw model mapped directly from Excel columns.</typeparam>
    /// <typeparam name="TOutput">The final resulting model after business logic and transformations.</typeparam>
    public interface IAsyncExcelProcessor<in TInput, TOutput>
    {
        /// <summary>
        /// Asynchronously processes and transforms the raw Excel input model into the desired output format.
        /// </summary>
        /// <param name="input">The raw data model extracted from a valid Excel row.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation, containing the transformed output model.</returns>
        Task<TOutput> ProcessAsync(TInput input, CancellationToken cancellationToken = default);
    }
}

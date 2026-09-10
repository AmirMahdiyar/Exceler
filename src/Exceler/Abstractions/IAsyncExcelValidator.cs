using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Exceler.Abstractions
{
    /// <summary>
    /// Defines an optional asynchronous business rule validator for the raw Excel input model.
    /// </summary>
    /// <typeparam name="TInput">The raw model mapped directly from Excel columns.</typeparam>
    public interface IAsyncExcelValidator<in TInput>
    {
        /// <summary>
        /// Asynchronously validates the populated input model and returns a collection of error messages.
        /// </summary>
        /// <param name="input">The populated input model to validate.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous validation operation, containing an enumerable of error messages. Return an empty collection if the model is valid.</returns>
        Task<IEnumerable<string>> ValidateAsync(TInput input, CancellationToken cancellationToken = default);
    }
}

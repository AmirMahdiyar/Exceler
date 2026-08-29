using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Pipeline.Read
{
    /// <summary>
    /// Abstract base class for the Chain of Responsibility pattern used in parsing, validating, and processing Excel rows.
    /// </summary>
    internal abstract class ReadHandler<TInput, TOutput> where TInput : class, new()
    {
        protected ReadHandler<TInput, TOutput>? Next;

        /// <summary>
        /// Sets the next handler in the processing chain.
        /// </summary>
        /// <param name="next">The next handler to execute.</param>
        /// <returns>The provided next handler, allowing for fluent chaining.</returns>
        public ReadHandler<TInput, TOutput> SetNext(ReadHandler<TInput, TOutput> next)
        {
            Next = next;
            return next;
        }

        /// <summary>
        /// Handles the current row context and optionally passes it to the next handler in the chain.
        /// </summary>
        /// <param name="context">The context containing row data and processing state.</param>
        public abstract void Handle(ReadContext<TInput, TOutput> context);
    }
}

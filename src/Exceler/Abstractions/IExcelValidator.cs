using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Abstractions
{
    /// <summary>
    /// Defines an optional business rule validator for the raw Excel input model.
    /// </summary>
    /// <typeparam name="TInput">The raw model mapped directly from Excel columns.</typeparam>
    public interface IExcelValidator<in TInput>
    {
        /// <summary>
        /// Validates the populated input model and returns a collection of error messages.
        /// </summary>
        /// <param name="input">The populated input model to validate.</param>
        /// <returns>An enumerable of error messages. Return an empty collection if the model is valid.</returns>
        IEnumerable<string> Validate(TInput input);
    }
}

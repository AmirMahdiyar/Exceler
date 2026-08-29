using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Abstractions
{
    /// <summary>
    /// Defines a processor that transforms the raw mapped Excel input model into a final Data Transfer Object (DTO) or domain entity.
    /// </summary>
    /// <typeparam name="TInput">The raw model mapped directly from Excel columns.</typeparam>
    /// <typeparam name="TOutput">The final resulting model after business logic and transformations.</typeparam>
    public interface IExcelProcessor<in TInput, out TOutput>
    {
        /// <summary>
        /// Processes and transforms the raw Excel input model into the desired output format.
        /// </summary>
        /// <param name="input">The raw data model extracted from a valid Excel row.</param>
        /// <returns>The transformed output model.</returns>
        TOutput Process(TInput input);
    }
}

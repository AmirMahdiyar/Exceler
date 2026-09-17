using Exceler.Abstractions;
using Exceler.Configuration;
using Exceler.Core;
using System;

namespace Exceler.Core.EPPlus
{
    /// <summary>
    /// EPPlus DOM-based implementation of <see cref="IExcelWriter"/>.
    /// Supports rich styling, formula recalculation, and runtime font-metric column auto-fitting.
    /// </summary>
    internal class EPPlusWriterEngine : DefaultWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EPPlusWriterEngine"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve mapping profiles.</param>
        public EPPlusWriterEngine(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}

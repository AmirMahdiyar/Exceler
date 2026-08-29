using Exceler.Abstractions;
using Exceler.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Pipeline.Read
{
    /// <summary>
    /// Encapsulates the state and data for a single Excel row as it passes through the reading pipeline.
    /// Acts as the payload in the Chain of Responsibility pattern.
    /// </summary>
    /// <typeparam name="TInput">The raw model representing a single Excel row.</typeparam>
    /// <typeparam name="TOutput">The final processed model mapped from the input.</typeparam>
    internal class ReadContext<TInput, TOutput> where TInput : class, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RowContext{TInput, TOutput}"/> class.
        /// </summary>
        /// <param name="row">The current row index being processed.</param>
        public ReadContext(int row)
        {
            Row = row;
            Result = new ExcelRowResult<TOutput> { RowIndex = row };
        }

        /// <summary>Gets the current worksheet being processed.</summary>
        public ExcelWorksheet Worksheet { get; init; } = null!;

        /// <summary>Gets the current row index (1-based).</summary>
        public int Row { get; init; }

        /// <summary>Gets the total number of columns in the worksheet.</summary>
        public int ColCount { get; init; }

        /// <summary>Gets the configuration profile containing mapping and styling rules.</summary>
        public ExcelProfile<TInput> Profile { get; init; } = null!;

        /// <summary>Gets the optional validator to validate the parsed input model.</summary>
        public IExcelValidator<TInput>? Validator { get; init; }

        /// <summary>Gets the processor responsible for transforming the input model into the output model.</summary>
        public IExcelProcessor<TInput, TOutput> Processor { get; init; } = null!;

        /// <summary>Gets the instance of the input model currently being populated.</summary>
        public TInput InputModel { get; } = new TInput();

        /// <summary>Gets the final result object containing parsed data or validation errors.</summary>
        public ExcelRowResult<TOutput> Result { get; }

    }
}

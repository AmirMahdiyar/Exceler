using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Abstractions
{
    /// <summary>
    /// Encapsulates the result of parsing a single row from an Excel worksheet.
    /// </summary>
    /// <typeparam name="TOutput">The type of the final processed output model.</typeparam>
    public class ExcelRowResult<TOutput>
    {
        /// <summary>
        /// Gets the 1-based index of the row in the Excel worksheet.
        /// </summary>
        public int RowIndex { get; init; }

        /// <summary>
        /// Gets or sets the processed data. This property is only populated if <see cref="IsValid"/> is true.
        /// </summary>
        public TOutput? Data { get; set; }

        /// <summary>
        /// Gets the collection of parsing or business validation errors encountered for this row.
        /// </summary>
        public List<string> Errors { get; } = new();

        /// <summary>
        /// Gets a value indicating whether the row was successfully parsed and validated without any errors.
        /// </summary>
        public bool IsValid => Errors.Count == 0;
    }
}

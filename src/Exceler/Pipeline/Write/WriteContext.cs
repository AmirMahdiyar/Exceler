using Exceler.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Pipeline.Write
{
    /// <summary>
    /// Encapsulates the state and data required to generate an Excel worksheet.
    /// Acts as the payload in the Chain of Responsibility pattern for the write pipeline.
    /// </summary>
    /// <typeparam name="TModel">The type of the model being exported.</typeparam>
    internal class WriteContext<TModel> where TModel : class, new()
    {
        /// <summary>Gets the target worksheet being generated.</summary>
        public ExcelWorksheet Worksheet { get; init; } = null!;

        /// <summary>Gets the configuration profile containing mapping, layout, and styling rules.</summary>
        public ExcelProfile<TModel> Profile { get; init; } = null!;

        /// <summary>Gets the collection of data records to be written to the worksheet.</summary>
        public IEnumerable<TModel> Data { get; init; } = null!;


        /// <summary>Gets or sets the total number of rows populated with data. Used by downstream handlers to apply range-specific styles.</summary>
        public int TotalRows { get; set; } = 1;
    }
}

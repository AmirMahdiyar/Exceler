using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Abstractions
{
    /// <summary>
    /// Provides mechanisms to generate Excel files from a collection of strongly typed objects.
    /// </summary>
    public interface IExcelWriter
    {
        /// <summary>
        /// Generates an Excel file as a byte array from the provided data collection based on the configured <see cref="ExcelProfile{TInput}"/>.
        /// </summary>
        /// <typeparam name="TModel">The type of the model being exported.</typeparam>
        /// <param name="data">The collection of models to write to the Excel file.</param>
        /// <returns>A byte array representing the generated Excel file (.xlsx format).</returns>
        Task<byte[]> Write<TModel>(
        IEnumerable<TModel> data,
        string? sheetName = null)
        where TModel : class, new();

        /// <summary>
        /// Generates an Excel file and writes it directly to the provided stream asynchronously.
        /// This method is highly recommended for large datasets as it prevents memory exhaustion (Zero Allocation).
        /// </summary>
        /// <typeparam name="TModel">The type of the model being exported.</typeparam>
        /// <param name="data">The collection of models to write to the Excel file.</param>
        /// <param name="outputStream">The stream where the Excel file will be written (e.g., Response.Body or MemoryStream).</param>
        /// <param name="sheetName">The optional name of the worksheet. If null, a default name is used.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        Task WriteAsync<TModel>(
            IEnumerable<TModel> data,
            Stream outputStream,
            string? sheetName = null)
            where TModel : class, new();
    }
}

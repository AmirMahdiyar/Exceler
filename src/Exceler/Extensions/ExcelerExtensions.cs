using Exceler.Abstractions;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Exceler.Extensions
{
    /// <summary>
    /// Provides high-level fluent extension methods for exporting collections and asynchronous streams to Excel workbooks.
    /// </summary>
    public static class ExcelerExtensions
    {
        /// <summary>
        /// Asynchronously exports an <see cref="IAsyncEnumerable{TModel}"/> stream (e.g. from EF Core AsAsyncEnumerable()) directly to an Excel output stream.
        /// Prevents buffering large datasets in memory by writing rows as they are streamed.
        /// </summary>
        /// <typeparam name="TModel">The type of the model representing Excel rows.</typeparam>
        /// <param name="dataStream">The asynchronous stream of models to write.</param>
        /// <param name="writer">The <see cref="IExcelWriter"/> service instance to use for workbook generation.</param>
        /// <param name="outputStream">The target output stream (e.g., FileStream, MemoryStream, or Response.Body) where the workbook is written.</param>
        /// <param name="sheetName">The optional name of the worksheet tab. If null or empty, a default name is used.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous export operation.</returns>
        public static async Task ToExcelAsync<TModel>(
            this IAsyncEnumerable<TModel> dataStream,
            IExcelWriter writer,
            Stream outputStream,
            string? sheetName = null) where TModel : class
        {
            await writer.WriteAsync(dataStream, outputStream, sheetName);
        }

        /// <summary>
        /// Asynchronously exports an <see cref="IEnumerable{TModel}"/> collection directly to an Excel output stream.
        /// </summary>
        /// <typeparam name="TModel">The type of the model representing Excel rows.</typeparam>
        /// <param name="data">The collection of models to write.</param>
        /// <param name="writer">The <see cref="IExcelWriter"/> service instance to use for workbook generation.</param>
        /// <param name="outputStream">The target output stream (e.g., FileStream, MemoryStream, or Response.Body) where the workbook is written.</param>
        /// <param name="sheetName">The optional name of the worksheet tab. If null or empty, a default name is used.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous export operation.</returns>
        public static async Task ToExcelAsync<TModel>(
            this IEnumerable<TModel> data,
            IExcelWriter writer,
            Stream outputStream,
            string? sheetName = null) where TModel : class
        {
            await writer.WriteAsync(data, outputStream, sheetName);
        }
    }
}

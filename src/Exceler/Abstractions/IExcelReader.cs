using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Abstractions
{
    /// <summary>
    /// Provides mechanisms to read and parse Excel files into strongly typed objects.
    /// </summary>
    public interface IExcelReader
    {
        /// <summary>
        /// Reads an Excel file stream synchronously and yields the parsed rows one by one.
        /// </summary>
        /// <typeparam name="TInput">The raw model representing a single Excel row.</typeparam>
        /// <typeparam name="TOutput">The final processed model mapped from the input.</typeparam>
        /// <param name="excelStream">The stream containing the Excel file data.</param>
        /// <returns>An enumerable collection of <see cref="ExcelRowResult{TOutput}"/> representing each row's processing status.</returns>
        IEnumerable<ExcelRowResult<TOutput>> Read<TInput, TOutput>(
                Stream excelStream,
                string? sheetName = null)
                where TInput : class, new();

        /// <summary>
        /// Reads a large Excel file stream asynchronously and yields the data in memory-efficient chunks.
        /// </summary>
        /// <typeparam name="TInput">The raw model representing a single Excel row.</typeparam>
        /// <typeparam name="TOutput">The final processed model mapped from the input.</typeparam>
        /// <param name="excelStream">The stream containing the Excel file data.</param>
        /// <param name="chunkSize">The maximum number of rows to return in a single chunk. Default is 10,000.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>An asynchronous stream of chunked results, ideal for bulk database insertions.</returns>
        IAsyncEnumerable<List<ExcelRowResult<TOutput>>> ReadInChunksAsync<TInput, TOutput>(
                Stream excelStream,
                int chunkSize = 10000,
                string? sheetName = null,
                CancellationToken cancellationToken = default)
                where TInput : class, new();
    }
}


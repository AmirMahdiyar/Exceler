using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using Exceler.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exceler.Core.OpenXml
{
    /// <summary>
    /// Encapsulates the streaming of OpenXML worksheet headers and data rows into <c>&lt;sheetData&gt;</c>
    /// using pre-compiled delegates, pre-computed column letters, and zero-allocation $O(1)$ style resolvers.
    /// </summary>
    /// <typeparam name="TModel">The row model type.</typeparam>
    internal sealed class OpenXmlRowStreamer<TModel> where TModel : class
    {
        private readonly KeyValuePair<int, Func<TModel, object>>[] _getters;
        private readonly string[] _colLetters;
        private readonly ColumnStyleResolver[] _resolvers;
        private readonly KeyValuePair<int, string>[] _headers;
        private readonly string[] _headerColLetters;
        private readonly uint _headerStyleIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenXmlRowStreamer{TModel}"/> class.
        /// </summary>
        /// <param name="profile">The model Excel profile containing mapping and styling rules.</param>
        /// <param name="styleManager">The style manager providing compiled style resolvers.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        public OpenXmlRowStreamer(ExcelProfile<TModel> profile, OpenXmlStyleManager styleManager)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }
            if (styleManager == null)
            {
                throw new ArgumentNullException(nameof(styleManager));
            }

            _getters = profile.CompiledGetters.OrderBy(g => g.Key).ToArray();
            _resolvers = _getters.Select(g => styleManager.GetResolver(g.Key)).ToArray();
            _colLetters = _getters.Select(g => SpreadsheetUtils.GetColumnLetter(g.Key)).ToArray();

            _headers = profile.ColumnHeaders.OrderBy(h => h.Key).ToArray();
            _headerColLetters = _headers.Select(h => SpreadsheetUtils.GetColumnLetter(h.Key)).ToArray();
            _headerStyleIndex = styleManager.HeaderStyleIndex;
        }

        /// <summary>
        /// Writes the header row if column headers are configured in the profile.
        /// </summary>
        /// <param name="writer">The SAX-based OpenXML writer.</param>
        /// <param name="currentRow">The current 1-based row index.</param>
        /// <returns>The updated next row index.</returns>
        public uint WriteHeaders(OpenXmlWriter writer, uint currentRow)
        {
            if (_headers.Length == 0)
            {
                return currentRow;
            }

            writer.WriteStartElement(new Row { RowIndex = currentRow });
            for (int i = 0; i < _headers.Length; i++)
            {
                var cellRef = $"{_headerColLetters[i]}{currentRow}";
                OpenXmlCellWriter.WriteStringCell(writer, cellRef, _headers[i].Value, _headerStyleIndex);
            }
            writer.WriteEndElement(); // Row
            return currentRow + 1;
        }

        /// <summary>
        /// Streams synchronous data items as OpenXML rows into the worksheet.
        /// </summary>
        /// <param name="writer">The SAX-based OpenXML writer.</param>
        /// <param name="startRow">The 1-based row index to start streaming at.</param>
        /// <param name="data">The collection of model instances.</param>
        /// <returns>The next row index after all items have been streamed.</returns>
        public uint WriteRows(OpenXmlWriter writer, uint startRow, IEnumerable<TModel>? data)
        {
            if (data == null)
            {
                return startRow;
            }

            uint currentRow = startRow;
            foreach (var item in data)
            {
                if (item == null)
                {
                    continue;
                }
                WriteDataRow(writer, currentRow, item);
                currentRow++;
            }

            return currentRow;
        }

        /// <summary>
        /// Streams asynchronous data items as OpenXML rows into the worksheet.
        /// </summary>
        /// <param name="writer">The SAX-based OpenXML writer.</param>
        /// <param name="startRow">The 1-based row index to start streaming at.</param>
        /// <param name="dataStream">The asynchronous stream of model instances.</param>
        /// <returns>The next row index after all items have been streamed.</returns>
        public async Task<uint> WriteRowsAsync(OpenXmlWriter writer, uint startRow, IAsyncEnumerable<TModel>? dataStream)
        {
            if (dataStream == null)
            {
                return startRow;
            }

            uint currentRow = startRow;
            await foreach (var item in dataStream)
            {
                if (item == null)
                {
                    continue;
                }
                WriteDataRow(writer, currentRow, item);
                currentRow++;
            }

            return currentRow;
        }

        /// <summary>
        /// Serializes a single model instance into an OpenXML worksheet row.
        /// </summary>
        /// <param name="writer">The SAX-based OpenXML writer.</param>
        /// <param name="row">The 1-based worksheet row index.</param>
        /// <param name="item">The data model instance.</param>
        public void WriteDataRow(OpenXmlWriter writer, uint row, TModel item)
        {
            writer.WriteStartElement(new Row { RowIndex = row });

            for (int i = 0; i < _getters.Length; i++)
            {
                var getter = _getters[i].Value;
                var styleIndex = _resolvers[i].ResolveStyle(item);
                var cellRef = $"{_colLetters[i]}{row}";

                var val = getter(item);
                OpenXmlCellWriter.WriteCell(writer, cellRef, val, styleIndex);
            }

            writer.WriteEndElement(); // Row
        }
    }
}

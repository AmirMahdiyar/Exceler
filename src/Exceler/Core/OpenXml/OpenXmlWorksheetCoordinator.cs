using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Exceler.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Exceler.Core.OpenXml
{
    /// <summary>
    /// Coordinates the sequential, schema-compliant serialization of an OpenXML worksheet,
    /// orchestrating layout views, column definitions, sheet data rows, and data validations
    /// in strict accordance with the ECMA-376 specification.
    /// </summary>
    internal static class OpenXmlWorksheetCoordinator
    {
        /// <summary>
        /// Writes the entire contents of a worksheet part using SAX streaming.
        /// </summary>
        /// <typeparam name="TModel">The row model type.</typeparam>
        /// <param name="worksheetPart">The OpenXML worksheet part to stream into.</param>
        /// <param name="profile">The model Excel profile.</param>
        /// <param name="rowStreamer">The row and cell streamer.</param>
        /// <param name="syncData">The optional synchronous data collection.</param>
        /// <param name="asyncDataStream">The optional asynchronous data stream.</param>
        /// <param name="refRanges">Optional reference sheet ranges for dropdowns.</param>
        /// <returns>A task representing the asynchronous worksheet serialization.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        public static async Task WriteWorksheetAsync<TModel>(
            WorksheetPart worksheetPart,
            ExcelProfile<TModel> profile,
            OpenXmlRowStreamer<TModel> rowStreamer,
            IEnumerable<TModel>? syncData,
            IAsyncEnumerable<TModel>? asyncDataStream,
            IReadOnlyDictionary<int, string>? refRanges) where TModel : class
        {
            if (worksheetPart == null)
            {
                throw new ArgumentNullException(nameof(worksheetPart));
            }
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }
            if (rowStreamer == null)
            {
                throw new ArgumentNullException(nameof(rowStreamer));
            }

            using var writer = OpenXmlWriter.Create(worksheetPart);
            writer.WriteStartElement(new Worksheet());

            // 1. sheetViews (RTL orientation if requested)
            OpenXmlSheetLayoutWriter.WriteSheetViews(writer, profile.RightToLeft);

            // 2. cols (Explicit column widths)
            OpenXmlSheetLayoutWriter.WriteColumns(writer, profile.ColumnStyles);

            // 3. sheetData (Row & cell streaming)
            writer.WriteStartElement(new SheetData());

            uint currentRow = 1;

            // Write header row if headers are configured
            currentRow = rowStreamer.WriteHeaders(writer, currentRow);

            // Stream data rows
            if (asyncDataStream != null)
            {
                currentRow = await rowStreamer.WriteRowsAsync(writer, currentRow, asyncDataStream);
            }
            else if (syncData != null)
            {
                currentRow = rowStreamer.WriteRows(writer, currentRow, syncData);
            }

            writer.WriteEndElement(); // SheetData

            // 4. dataValidations (Dropdown lists in strict ECMA-376 schema order)
            OpenXmlDataValidationWriter.WriteDataValidations(writer, profile.ColumnStyles, (int)currentRow - 1, refRanges);

            writer.WriteEndElement(); // Worksheet
        }
    }
}

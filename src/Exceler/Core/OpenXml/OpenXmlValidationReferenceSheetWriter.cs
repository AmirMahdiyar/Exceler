using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Exceler.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Exceler.Core.OpenXml
{
    /// <summary>
    /// Encapsulates the serialization of the hidden reference worksheet (<c>_ValidationData</c>)
    /// populated with dropdown options exceeding inline limits or containing commas.
    /// </summary>
    internal static class OpenXmlValidationReferenceSheetWriter
    {
        /// <summary>
        /// Populates the hidden reference worksheet with options for dropdown columns
        /// and records the corresponding Excel range formula in <paramref name="refRanges"/>.
        /// </summary>
        /// <param name="refPart">The worksheet part dedicated to validation reference data.</param>
        /// <param name="refDropdownColumns">The collection of column index to style pairs requiring reference sheets.</param>
        /// <param name="refRanges">The dictionary populated with column index to range formula mappings.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        public static void WriteReferenceSheet(
            WorksheetPart refPart,
            IReadOnlyList<KeyValuePair<int, ColumnStyle>> refDropdownColumns,
            IDictionary<int, string> refRanges)
        {
            if (refPart == null)
            {
                throw new ArgumentNullException(nameof(refPart));
            }
            if (refDropdownColumns == null)
            {
                throw new ArgumentNullException(nameof(refDropdownColumns));
            }
            if (refRanges == null)
            {
                throw new ArgumentNullException(nameof(refRanges));
            }

            if (refDropdownColumns.Count == 0)
            {
                return;
            }

            using var writer = OpenXmlWriter.Create(refPart);
            writer.WriteStartElement(new Worksheet());
            writer.WriteStartElement(new SheetData());

            var colData = new List<(string ColLetter, IReadOnlyList<string> Options)>(refDropdownColumns.Count);
            for (int i = 0; i < refDropdownColumns.Count; i++)
            {
                string refColLetter = SpreadsheetUtils.GetColumnLetter(i + 1);
                var dropdown = refDropdownColumns[i].Value.Dropdown!;
                refRanges[refDropdownColumns[i].Key] = $"'_ValidationData'!${refColLetter}$1:${refColLetter}${dropdown.Options.Count}";
                colData.Add((refColLetter, dropdown.Options));
            }

            int maxRows = colData.Max(c => c.Options.Count);
            for (uint r = 1; r <= maxRows; r++)
            {
                writer.WriteStartElement(new Row { RowIndex = r });
                foreach (var col in colData)
                {
                    if (r <= col.Options.Count)
                    {
                        string cellRef = $"{col.ColLetter}{r}";
                        OpenXmlCellWriter.WriteStringCell(writer, cellRef, col.Options[(int)r - 1], 0);
                    }
                }
                writer.WriteEndElement(); // Row
            }

            writer.WriteEndElement(); // SheetData
            writer.WriteEndElement(); // Worksheet
        }
    }
}

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using Exceler.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Exceler.Core.OpenXml
{
    /// <summary>
    /// Encapsulates the serialization of worksheet structural layout and view elements
    /// strictly adhering to the ECMA-376 schema sequencing rules (<c>&lt;sheetViews&gt;</c> followed by <c>&lt;cols&gt;</c>).
    /// </summary>
    internal static class OpenXmlSheetLayoutWriter
    {
        /// <summary>
        /// Writes the <c>&lt;sheetViews&gt;</c> element if right-to-left orientation is requested.
        /// </summary>
        /// <param name="writer">The SAX-based OpenXML writer for the worksheet.</param>
        /// <param name="rightToLeft">True if right-to-left layout is enabled; otherwise false.</param>
        public static void WriteSheetViews(OpenXmlWriter writer, bool rightToLeft)
        {
            if (rightToLeft)
            {
                writer.WriteElement(new SheetViews(
                    new SheetView { WorkbookViewId = 0U, RightToLeft = true }
                ));
            }
        }

        /// <summary>
        /// Writes the <c>&lt;cols&gt;</c> element configuring explicit column widths from the mapped column styles.
        /// </summary>
        /// <param name="writer">The SAX-based OpenXML writer for the worksheet.</param>
        /// <param name="columnStyles">The dictionary mapping 1-based column indices to their styles.</param>
        public static void WriteColumns(OpenXmlWriter writer, IReadOnlyDictionary<int, ColumnStyle> columnStyles)
        {
            var columns = new Columns();
            foreach (var kvp in columnStyles)
            {
                if (kvp.Value.Width.HasValue)
                {
                    columns.Append(new Column
                    {
                        Min = (uint)kvp.Key,
                        Max = (uint)kvp.Key,
                        Width = kvp.Value.Width.Value,
                        CustomWidth = true
                    });
                }
            }

            if (columns.Any())
            {
                writer.WriteElement(columns);
            }
        }
    }
}

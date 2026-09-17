using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Globalization;

namespace Exceler.Core.OpenXml
{
    /// <summary>
    /// Provides low-level, high-performance serialization of CLR values into ECMA-376 OpenXML cell elements.
    /// Employs forward-only SAX streaming via <see cref="OpenXmlWriter"/> and inline string structures
    /// (<see cref="CellValues.InlineString"/>) to achieve $O(1)$ memory allocation.
    /// </summary>
    internal static class OpenXmlCellWriter
    {
        /// <summary>
        /// Dispatches and writes a cell value with the specified cell reference and style index
        /// into the target <see cref="OpenXmlWriter"/> stream.
        /// </summary>
        /// <param name="writer">The SAX-based OpenXML writer for the current worksheet part.</param>
        /// <param name="cellRef">The A1-notation cell coordinate (e.g., "A1", "C25").</param>
        /// <param name="value">The boxed CLR property value to serialize.</param>
        /// <param name="styleIndex">The zero-based style index in the workbook stylesheet.</param>
        public static void WriteCell(OpenXmlWriter writer, string cellRef, object? value, uint styleIndex)
        {
            if (value == null || value == DBNull.Value)
            {
                WriteEmptyCell(writer, cellRef, styleIndex);
                return;
            }

            switch (value)
            {
                case string str:
                    WriteStringCell(writer, cellRef, str, styleIndex);
                    break;

                case bool b:
                    WriteBooleanCell(writer, cellRef, b, styleIndex);
                    break;

                case DateOnly dateOnly:
                    double dateOa = dateOnly.ToDateTime(TimeOnly.MinValue).ToOADate();
                    WriteNumberCell(writer, cellRef, dateOa.ToString(CultureInfo.InvariantCulture), styleIndex);
                    break;

                case TimeOnly timeOnly:
                    double timeFraction = timeOnly.ToTimeSpan().TotalDays;
                    WriteNumberCell(writer, cellRef, timeFraction.ToString(CultureInfo.InvariantCulture), styleIndex);
                    break;

                case DateTime dt:
                    double dtOa = dt.ToOADate();
                    WriteNumberCell(writer, cellRef, dtOa.ToString(CultureInfo.InvariantCulture), styleIndex);
                    break;

                case DateTimeOffset dto:
                    double dtoOa = dto.DateTime.ToOADate();
                    WriteNumberCell(writer, cellRef, dtoOa.ToString(CultureInfo.InvariantCulture), styleIndex);
                    break;

                case TimeSpan ts:
                    double tsFraction = ts.TotalDays;
                    WriteNumberCell(writer, cellRef, tsFraction.ToString(CultureInfo.InvariantCulture), styleIndex);
                    break;

                case byte or sbyte or short or ushort or int or uint or long or ulong:
                    WriteNumberCell(writer, cellRef, value.ToString()!, styleIndex);
                    break;

                case float f:
                    WriteNumberCell(writer, cellRef, f.ToString("G9", CultureInfo.InvariantCulture), styleIndex);
                    break;

                case double d:
                    WriteNumberCell(writer, cellRef, d.ToString("G17", CultureInfo.InvariantCulture), styleIndex);
                    break;

                case decimal dec:
                    WriteNumberCell(writer, cellRef, dec.ToString(CultureInfo.InvariantCulture), styleIndex);
                    break;

                default:
                    WriteStringCell(writer, cellRef, value.ToString(), styleIndex);
                    break;
            }
        }

        /// <summary>
        /// Writes an inline string cell (<c>&lt;c t="inlineStr"&gt;&lt;is&gt;&lt;t&gt;...&lt;/t&gt;&lt;/is&gt;&lt;/c&gt;</c>)
        /// directly into the SAX stream. Avoids SharedStringTable allocations.
        /// </summary>
        /// <param name="writer">The SAX-based OpenXML writer.</param>
        /// <param name="cellRef">The cell coordinate reference.</param>
        /// <param name="value">The string value to write.</param>
        /// <param name="styleIndex">The style index to apply to the cell.</param>
        public static void WriteStringCell(OpenXmlWriter writer, string cellRef, string? value, uint styleIndex)
        {
            writer.WriteStartElement(new Cell
            {
                CellReference = cellRef,
                StyleIndex = styleIndex,
                DataType = CellValues.InlineString
            });
            writer.WriteStartElement(new InlineString());
            writer.WriteElement(new Text(value ?? string.Empty));
            writer.WriteEndElement(); // </is>
            writer.WriteEndElement(); // </c>
        }

        /// <summary>
        /// Writes a numeric cell (<c>&lt;c&gt;&lt;v&gt;...&lt;/v&gt;&lt;/c&gt;</c>) into the SAX stream.
        /// </summary>
        /// <param name="writer">The SAX-based OpenXML writer.</param>
        /// <param name="cellRef">The cell coordinate reference.</param>
        /// <param name="numberStr">The invariant-culture formatted number representation.</param>
        /// <param name="styleIndex">The style index to apply to the cell.</param>
        public static void WriteNumberCell(OpenXmlWriter writer, string cellRef, string numberStr, uint styleIndex)
        {
            writer.WriteStartElement(new Cell
            {
                CellReference = cellRef,
                StyleIndex = styleIndex,
                DataType = CellValues.Number
            });
            writer.WriteElement(new CellValue(numberStr));
            writer.WriteEndElement(); // </c>
        }

        /// <summary>
        /// Writes a boolean cell (<c>&lt;c t="b"&gt;&lt;v&gt;1|0&lt;/v&gt;&lt;/c&gt;</c>) into the SAX stream.
        /// </summary>
        /// <param name="writer">The SAX-based OpenXML writer.</param>
        /// <param name="cellRef">The cell coordinate reference.</param>
        /// <param name="value">The boolean value.</param>
        /// <param name="styleIndex">The style index to apply to the cell.</param>
        public static void WriteBooleanCell(OpenXmlWriter writer, string cellRef, bool value, uint styleIndex)
        {
            writer.WriteStartElement(new Cell
            {
                CellReference = cellRef,
                StyleIndex = styleIndex,
                DataType = CellValues.Boolean
            });
            writer.WriteElement(new CellValue(value ? "1" : "0"));
            writer.WriteEndElement(); // </c>
        }

        /// <summary>
        /// Writes an empty cell (<c>&lt;c s="N"/&gt;</c>) if a non-zero style is applied.
        /// Unstyled empty cells are omitted to keep the generated XML minimal and fast.
        /// </summary>
        /// <param name="writer">The SAX-based OpenXML writer.</param>
        /// <param name="cellRef">The cell coordinate reference.</param>
        /// <param name="styleIndex">The style index to apply.</param>
        public static void WriteEmptyCell(OpenXmlWriter writer, string cellRef, uint styleIndex)
        {
            if (styleIndex != 0)
            {
                writer.WriteStartElement(new Cell
                {
                    CellReference = cellRef,
                    StyleIndex = styleIndex
                });
                writer.WriteEndElement(); // </c>
            }
        }
    }
}

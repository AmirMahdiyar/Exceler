using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using Exceler.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Exceler.Core.OpenXml
{
    /// <summary>
    /// Encapsulates the serialization of OpenXML <see cref="DataValidations"/> elements in strict accordance
    /// with the ECMA-376 schema sequencing (written immediately following <c>&lt;sheetData&gt;</c>).
    /// </summary>
    internal static class OpenXmlDataValidationWriter
    {
        /// <summary>
        /// Writes the <c>&lt;dataValidations&gt;</c> element and all child <see cref="DataValidation"/> elements
        /// for worksheet columns configured with a dropdown list.
        /// </summary>
        /// <param name="writer">The SAX-based OpenXML writer for the worksheet.</param>
        /// <param name="columnStyles">The dictionary of mapped column styles.</param>
        /// <param name="totalDataRows">The total number of rows present in the exported sheet.</param>
        /// <param name="refRanges">An optional mapping of column index to reference sheet range formula (e.g., <c>'_ValidationData'!$A$1:$A$50</c>).</param>
        public static void WriteDataValidations(
            OpenXmlWriter writer,
            IReadOnlyDictionary<int, ColumnStyle> columnStyles,
            int totalDataRows,
            IReadOnlyDictionary<int, string>? refRanges = null)
        {
            var dropdownColumns = columnStyles
                .Where(kvp => kvp.Value.Dropdown != null)
                .OrderBy(kvp => kvp.Key)
                .ToList();

            if (dropdownColumns.Count == 0)
                return;

            writer.WriteStartElement(new DataValidations { Count = (uint)dropdownColumns.Count });

            foreach (var kvp in dropdownColumns)
            {
                int colIndex = kvp.Key;
                var dropdown = kvp.Value.Dropdown!;

                string colLetter = SpreadsheetUtils.GetColumnLetter(colIndex);
                int startRow = dropdown.StartRow;
                int endRow = Math.Max(totalDataRows, dropdown.EndRow);
                string cellRange = $"{colLetter}{startRow}:{colLetter}{endRow}";

                string formula1;
                if (refRanges != null && refRanges.TryGetValue(colIndex, out var externalRange))
                {
                    formula1 = externalRange;
                }
                else
                {
                    formula1 = dropdown.GetInlineFormula();
                }

                var validation = new DataValidation
                {
                    Type = DataValidationValues.List,
                    AllowBlank = dropdown.AllowBlank,
                    ShowErrorMessage = dropdown.ShowErrorMessage,
                    ShowInputMessage = dropdown.ShowInputMessage,
                    SequenceOfReferences = new ListValue<StringValue> { InnerText = cellRange }
                };

                if (!string.IsNullOrEmpty(dropdown.ErrorTitle))
                    validation.ErrorTitle = dropdown.ErrorTitle;

                if (!string.IsNullOrEmpty(dropdown.ErrorMessage))
                    validation.Error = dropdown.ErrorMessage;

                if (!string.IsNullOrEmpty(dropdown.InputTitle))
                    validation.PromptTitle = dropdown.InputTitle;

                if (!string.IsNullOrEmpty(dropdown.InputMessage))
                    validation.Prompt = dropdown.InputMessage;

                validation.Append(new Formula1(formula1));

                writer.WriteElement(validation);
            }

            writer.WriteEndElement(); // </dataValidations>
        }
    }
}

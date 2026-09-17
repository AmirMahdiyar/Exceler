using System;
using System.Text;

namespace Exceler.Core
{
    /// <summary>
    /// Provides utility methods for spreadsheet calculations, column address translations,
    /// and OpenXML worksheet naming constraints.
    /// </summary>
    internal static class SpreadsheetUtils
    {
        private static readonly char[] InvalidSheetNameCharacters = { '\\', '/', '?', '*', '[', ']', ':' };

        /// <summary>
        /// Converts a 1-based column index to its corresponding Excel column letter notation (e.g., 1 -> "A", 27 -> "AA").
        /// </summary>
        /// <param name="columnIndex">The 1-based column index (must be greater than or equal to 1).</param>
        /// <returns>The alphabetical column reference corresponding to the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="columnIndex"/> is less than 1.</exception>
        public static string GetColumnLetter(int columnIndex)
        {
            if (columnIndex < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, "Column index must be greater than or equal to 1.");
            }

            int div = columnIndex;
            string col = string.Empty;
            while (div > 0)
            {
                int mod = (div - 1) % 26;
                col = (char)(65 + mod) + col;
                div = (div - mod) / 26;
            }
            return col;
        }

        /// <summary>
        /// Sanitizes and validates an Excel worksheet name according to Microsoft Excel and ECMA-376 specifications.
        /// Replaces invalid characters (<c>\ / ? * [ ] :</c>) with underscores, trims whitespace and single quotes,
        /// and enforces the maximum length limit of 31 characters.
        /// </summary>
        /// <param name="sheetName">The requested sheet name, which may be null or contain invalid characters.</param>
        /// <returns>A valid, sanitized sheet name safe for inclusion in the workbook. Defaults to "Sheet1" if input is blank.</returns>
        public static string SanitizeSheetName(string? sheetName)
        {
            if (string.IsNullOrWhiteSpace(sheetName))
            {
                return "Sheet1";
            }

            var sanitized = new StringBuilder(sheetName.Length);
            foreach (char c in sheetName)
            {
                if (Array.IndexOf(InvalidSheetNameCharacters, c) >= 0)
                {
                    sanitized.Append('_');
                }
                else
                {
                    sanitized.Append(c);
                }
            }

            var result = sanitized.ToString().Trim().Trim('\'');
            if (string.IsNullOrEmpty(result))
            {
                return "Sheet1";
            }

            return result.Length > 31 ? result.Substring(0, 31) : result;
        }
    }
}

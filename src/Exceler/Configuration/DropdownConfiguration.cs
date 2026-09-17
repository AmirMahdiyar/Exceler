using System;
using System.Collections.Generic;
using System.Linq;

namespace Exceler.Configuration
{
    /// <summary>
    /// Represents the configuration for an Excel dropdown list (Data Validation) applied to a worksheet column.
    /// Supports automatic routing between inline list formulas and hidden reference worksheets based on length and character constraints.
    /// </summary>
    public class DropdownConfiguration
    {
        private int _startRow = 2;
        private int _endRow = 1000;

        /// <summary>
        /// Gets the distinct list of valid dropdown options.
        /// </summary>
        public IReadOnlyList<string> Options { get; }

        /// <summary>
        /// Gets or sets a value indicating whether blank or empty cells are considered valid.
        /// Defaults to <c>true</c>.
        /// </summary>
        public bool AllowBlank { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether an error alert message is displayed when an invalid value is entered.
        /// Defaults to <c>true</c>.
        /// </summary>
        public bool ShowErrorMessage { get; set; } = true;

        /// <summary>
        /// Gets or sets the title of the error alert dialog displayed when an invalid value is entered.
        /// </summary>
        public string? ErrorTitle { get; set; }

        /// <summary>
        /// Gets or sets the error message text displayed when an invalid value is entered.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an input prompt message is displayed when the cell is selected.
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool ShowInputMessage { get; set; } = false;

        /// <summary>
        /// Gets or sets the title of the input prompt dialog displayed when the cell is selected.
        /// </summary>
        public string? InputTitle { get; set; }

        /// <summary>
        /// Gets or sets the input prompt text displayed when the cell is selected.
        /// </summary>
        public string? InputMessage { get; set; }

        /// <summary>
        /// Gets or sets the 1-based start row index for the dropdown validation range.
        /// Defaults to <c>2</c> (the first row immediately following the header).
        /// </summary>
        public int StartRow
        {
            get => _startRow;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "StartRow must be greater than or equal to 1.");
                _startRow = value;
            }
        }

        /// <summary>
        /// Gets or sets the 1-based end row index for the dropdown validation range.
        /// Defaults to <c>1000</c> for template entry. Must be greater than or equal to <see cref="StartRow"/>.
        /// </summary>
        public int EndRow
        {
            get => _endRow;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "EndRow must be greater than or equal to 1.");
                _endRow = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the dropdown list must be stored in a hidden reference worksheet
        /// rather than as an inline string literal. Returns <c>true</c> if the joined options string exceeds
        /// 255 characters or if any option contains a comma (which corrupts Excel's inline list delimiter).
        /// </summary>
        public bool RequiresReferenceSheet
        {
            get
            {
                // In Excel, formula1 string literal limit is 255 characters (including quotes and commas).
                // Also, commas inside an item break Excel's inline parsing because comma is the item separator.
                int totalLength = Options.Sum(o => o.Length) + Math.Max(0, Options.Count - 1) + 2; // +2 for enclosing quotes
                return totalLength > 255 || Options.Any(o => o.Contains(','));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DropdownConfiguration"/> class with the specified options.
        /// </summary>
        /// <param name="options">The collection of options to display in the dropdown.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="options"/> is empty or contains only whitespace values.</exception>
        public DropdownConfiguration(IEnumerable<string> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var sanitized = options
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (sanitized.Count == 0)
                throw new ArgumentException("Dropdown options cannot be empty or contain only whitespace.", nameof(options));

            Options = sanitized;
        }

        /// <summary>
        /// Generates the inline Excel list formula string (e.g., <c>"\"Pending,Approved,Rejected\""</c>).
        /// </summary>
        /// <returns>A quoted, comma-separated list of items safe for inline data validation.</returns>
        public string GetInlineFormula()
        {
            return $"\"{string.Join(",", Options)}\"";
        }
    }
}

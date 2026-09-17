using Exceler.Core;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exceler.Pipeline.Write.Handlers
{
    /// <summary>
    /// Responsible for configuring Excel Data Validation (dropdown lists) on worksheet columns in the EPPlus pipeline.
    /// Automatically routes between inline list values and hidden reference worksheets (<c>_ValidationData</c>)
    /// based on string length and comma delimiters.
    /// </summary>
    /// <typeparam name="TModel">The type of the model being exported.</typeparam>
    internal class DataValidationWriterHandler<TModel> : WriteHandler<TModel> where TModel : class
    {
        /// <inheritdoc />
        public override async Task HandleAsync(WriteContext<TModel> context)
        {
            var dropdownColumns = context.Profile.ColumnStyles
                .Where(kvp => kvp.Value.Dropdown != null)
                .OrderBy(kvp => kvp.Key)
                .ToList();

            if (dropdownColumns.Count > 0)
            {
                var package = context.Worksheet.Workbook;
                var refDropdowns = dropdownColumns
                    .Where(kvp => kvp.Value.Dropdown!.RequiresReferenceSheet)
                    .ToList();

                var refRanges = new Dictionary<int, string>();

                if (refDropdowns.Count > 0)
                {
                    var refWorksheet = package.Worksheets["_ValidationData"] ?? package.Worksheets.Add("_ValidationData");
                    refWorksheet.Hidden = eWorkSheetHidden.Hidden;

                    for (int i = 0; i < refDropdowns.Count; i++)
                    {
                        int refCol = i + 1;
                        string refColLetter = SpreadsheetUtils.GetColumnLetter(refCol);
                        var dropdown = refDropdowns[i].Value.Dropdown!;

                        for (int r = 0; r < dropdown.Options.Count; r++)
                        {
                            refWorksheet.Cells[r + 1, refCol].Value = dropdown.Options[r];
                        }

                        refRanges[refDropdowns[i].Key] = $"'_ValidationData'!${refColLetter}$1:${refColLetter}${dropdown.Options.Count}";
                    }
                }

                foreach (var kvp in dropdownColumns)
                {
                    int colIndex = kvp.Key;
                    var dropdown = kvp.Value.Dropdown!;

                    int startRow = dropdown.StartRow;
                    int endRow = Math.Max(context.TotalRows, dropdown.EndRow);
                    string colLetter = SpreadsheetUtils.GetColumnLetter(colIndex);
                    string address = $"{colLetter}{startRow}:{colLetter}{endRow}";

                    var validation = context.Worksheet.DataValidations.AddListValidation(address);
                    validation.AllowBlank = dropdown.AllowBlank;
                    validation.ShowErrorMessage = dropdown.ShowErrorMessage;
                    validation.ShowInputMessage = dropdown.ShowInputMessage;

                    if (!string.IsNullOrEmpty(dropdown.ErrorTitle))
                        validation.ErrorTitle = dropdown.ErrorTitle;

                    if (!string.IsNullOrEmpty(dropdown.ErrorMessage))
                        validation.Error = dropdown.ErrorMessage;

                    if (!string.IsNullOrEmpty(dropdown.InputTitle))
                        validation.PromptTitle = dropdown.InputTitle;

                    if (!string.IsNullOrEmpty(dropdown.InputMessage))
                        validation.Prompt = dropdown.InputMessage;

                    if (refRanges.TryGetValue(colIndex, out var rangeFormula))
                    {
                        validation.Formula.ExcelFormula = rangeFormula;
                    }
                    else
                    {
                        foreach (var opt in dropdown.Options)
                        {
                            validation.Formula.Values.Add(opt);
                        }
                    }
                }
            }

            if (Next is not null)
                await Next.HandleAsync(context);
        }
    }
}

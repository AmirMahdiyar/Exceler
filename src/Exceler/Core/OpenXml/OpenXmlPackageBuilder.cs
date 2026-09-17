using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.IO;

namespace Exceler.Core.OpenXml
{
    /// <summary>
    /// Implements the Builder pattern for initializing, assembling, and persisting the OpenXML
    /// spreadsheet package (<see cref="SpreadsheetDocument"/>), its <see cref="WorkbookPart"/>,
    /// <see cref="WorkbookStylesPart"/>, and sheet registry (<see cref="Sheets"/>).
    /// </summary>
    internal sealed class OpenXmlPackageBuilder : IDisposable
    {
        private readonly SpreadsheetDocument _document;
        private readonly WorkbookPart _workbookPart;
        private readonly Sheets _sheets;
        private uint _nextSheetId = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenXmlPackageBuilder"/> class bound to an output stream.
        /// </summary>
        /// <param name="outputStream">The stream where the spreadsheet package is written.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="outputStream"/> is null.</exception>
        public OpenXmlPackageBuilder(Stream outputStream)
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }

            _document = SpreadsheetDocument.Create(outputStream, SpreadsheetDocumentType.Workbook);
            _workbookPart = _document.AddWorkbookPart();
            _workbookPart.Workbook = new Workbook();
            _sheets = _workbookPart.Workbook.AppendChild(new Sheets());
        }

        /// <summary>
        /// Configures and registers the workbook stylesheet part.
        /// </summary>
        /// <param name="stylesheet">The compiled stylesheet to assign.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stylesheet"/> is null.</exception>
        public void SetStylesheet(Stylesheet stylesheet)
        {
            if (stylesheet == null)
            {
                throw new ArgumentNullException(nameof(stylesheet));
            }

            var stylesPart = _workbookPart.AddNewPart<WorkbookStylesPart>();
            stylesPart.Stylesheet = stylesheet;
            stylesPart.Stylesheet.Save();
        }

        /// <summary>
        /// Creates and attaches a new <see cref="WorksheetPart"/> to the workbook.
        /// </summary>
        /// <returns>The newly created worksheet part.</returns>
        public WorksheetPart CreateWorksheetPart()
        {
            return _workbookPart.AddNewPart<WorksheetPart>();
        }

        /// <summary>
        /// Registers a worksheet part in the workbook's <see cref="Sheets"/> collection with an optional visibility state.
        /// </summary>
        /// <param name="worksheetPart">The worksheet part to register.</param>
        /// <param name="sheetName">The name to assign to the sheet (sanitized automatically).</param>
        /// <param name="state">Optional visibility state (e.g. <see cref="SheetStateValues.Hidden"/>).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="worksheetPart"/> is null.</exception>
        public void RegisterSheet(WorksheetPart worksheetPart, string? sheetName, SheetStateValues? state = null)
        {
            if (worksheetPart == null)
            {
                throw new ArgumentNullException(nameof(worksheetPart));
            }

            var sheet = new Sheet
            {
                Id = _workbookPart.GetIdOfPart(worksheetPart),
                SheetId = _nextSheetId++,
                Name = SpreadsheetUtils.SanitizeSheetName(sheetName)
            };

            if (state.HasValue)
            {
                sheet.State = state.Value;
            }

            _sheets.Append(sheet);
        }

        /// <summary>
        /// Saves all pending workbook structural changes.
        /// </summary>
        public void Save()
        {
            _workbookPart.Workbook.Save();
        }

        /// <summary>
        /// Disposes the underlying <see cref="SpreadsheetDocument"/> and flushes the package.
        /// </summary>
        public void Dispose()
        {
            _document.Dispose();
        }
    }
}

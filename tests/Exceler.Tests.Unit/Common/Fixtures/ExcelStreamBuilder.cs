using OfficeOpenXml;

namespace Exceler.Tests.Common.Fixtures
{
    public class ExcelStreamBuilder : IDisposable
    {
        private readonly ExcelPackage _package;
        private readonly ExcelWorksheet _worksheet;

        public ExcelStreamBuilder(string sheetName = "Sheet1")
        {
            _package = new ExcelPackage();
            _worksheet = _package.Workbook.Worksheets.Add(sheetName);
        }

        public ExcelStreamBuilder WithTestModelHeaders()
            => WithHeaders("User ID", "Full Name", "Account Balance", "Creation Date");

        public ExcelStreamBuilder WithHeaders(params string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                _worksheet.Cells[1, i + 1].Value = headers[i];
            }
            return this;
        }

        public ExcelStreamBuilder WithRow(int rowIndex, params object?[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                _worksheet.Cells[rowIndex, i + 1].Value = values[i];
            }
            return this;
        }

        public ExcelStreamBuilder WithCell(int row, int col, object? value)
        {
            _worksheet.Cells[row, col].Value = value;
            return this;
        }

        public ExcelStreamBuilder WithFormula(int row, int col, string formula)
        {
            _worksheet.Cells[row, col].Formula = formula;
            _worksheet.Calculate();
            return this;
        }

        public MemoryStream Build()
        {
            var stream = new MemoryStream();
            _package.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Creates an in-memory Excel stream containing a single empty worksheet with the specified name.
        /// </summary>
        /// <param name="sheetName">The name of the empty worksheet.</param>
        /// <returns>A readable <see cref="MemoryStream"/> of the workbook.</returns>
        public static MemoryStream EmptySheet(string sheetName = "Sheet1")
        {
            using var builder = new ExcelStreamBuilder(sheetName);
            return builder.Build();
        }

        public void Dispose()
        {
            _package.Dispose();
        }
    }
}

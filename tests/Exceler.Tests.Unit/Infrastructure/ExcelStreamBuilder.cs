using OfficeOpenXml;

namespace Exceler.Tests.Infrastructure
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

        public void Dispose()
        {
            _package.Dispose();
        }
    }
}

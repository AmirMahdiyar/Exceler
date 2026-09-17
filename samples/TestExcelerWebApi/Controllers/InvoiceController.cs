using Exceler.Abstractions;
using Microsoft.AspNetCore.Mvc;
using TestExcelerWebApi.Inputs;

namespace TestExcelerWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IExcelWriter _excelWriter;

        public InvoiceController(IExcelWriter excelWriter)
        {
            _excelWriter = excelWriter;
        }

        /// <summary>
        /// Exports an invoice template featuring Smart Dropdowns powered by the EPPlus engine.
        /// Demonstrates enum-driven dropdowns ('Type') and comma-safe list dropdowns ('Deliverer').
        /// </summary>
        [HttpGet("export-template-with-dropdowns")]
        public async Task<IActionResult> ExportTemplateWithDropdowns()
        {
            var inputs = new List<InvoiceExcelInput>
            {
                new InvoiceExcelInput { Id = 1, Type = nameof(InvoiceType.Standard), DeliveredTime = DateTime.Now.AddDays(1), Deliverer = "Ali", DlNumber = "4887895" },
                new InvoiceExcelInput { Id = 2, Type = nameof(InvoiceType.Commercial), DeliveredTime = DateTime.Now.AddDays(2), Deliverer = "Mohammad, Head Courier", DlNumber = "4587512" },
                new InvoiceExcelInput { Id = 3, Type = nameof(InvoiceType.Proforma), DeliveredTime = DateTime.Now, Deliverer = "Reza", DlNumber = "999999" }
            };

            var memoryStream = new MemoryStream();
            await _excelWriter.WriteAsync(inputs, memoryStream);
            memoryStream.Position = 0;

            return File(
                memoryStream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Invoice_Template_With_Dropdowns.xlsx"
            );
        }

        /// <summary>
        /// Exports a demonstration workbook illustrating C#-based Conditional Styling with EPPlus:
        /// - Row-level: Deliverer "Mohammad, Head Courier" highlights the ENTIRE ROW in SoftYellow.
        /// - Column-level: 'Type' Commercial is SoftGreen with bold text.
        /// - Column-level: 'Type' CreditNote is SoftRed with bold DarkRed text.
        /// - Cascading Precedence: Column styles override row styles on specific cells!
        /// </summary>
        [HttpGet("export-conditional-styles-demo")]
        public async Task<IActionResult> ExportConditionalStylesDemo()
        {
            var inputs = new List<InvoiceExcelInput>
            {
                // Case 1: Standard invoice with base style
                new() { Id = 1, Type = nameof(InvoiceType.Standard), DeliveredTime = DateTime.Now.AddDays(1), Deliverer = "Ali", DlNumber = "4887895" },
                // Case 2: Row-level rule (SoftYellow row) + Column-level Commercial (SoftGreen overrides row on Type cell!)
                new() { Id = 2, Type = nameof(InvoiceType.Commercial), DeliveredTime = DateTime.Now.AddDays(2), Deliverer = "Mohammad, Head Courier", DlNumber = "4587512" },
                // Case 3: Row-level rule (SoftYellow row) + Column-level CreditNote (SoftRed overrides row on Type cell!)
                new() { Id = 3, Type = nameof(InvoiceType.CreditNote), DeliveredTime = DateTime.Now.AddDays(3), Deliverer = "Mohammad, Head Courier", DlNumber = "1234567" },
                // Case 4: Column-level CreditNote without row rule (SoftRed cell only)
                new() { Id = 4, Type = nameof(InvoiceType.CreditNote), DeliveredTime = DateTime.Now.AddDays(4), Deliverer = "Reza", DlNumber = "999999" },
                // Case 5: Column-level Commercial without row rule (SoftGreen cell only)
                new() { Id = 5, Type = nameof(InvoiceType.Commercial), DeliveredTime = DateTime.Now.AddDays(5), Deliverer = "Akbar", DlNumber = "888888" }
            };

            var memoryStream = new MemoryStream();
            await _excelWriter.WriteAsync(inputs, memoryStream, sheetName: "Conditional Styles Demo");
            memoryStream.Position = 0;

            return File(
                memoryStream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Invoice_Conditional_Styles_Demo.xlsx"
            );
        }

        [HttpPost]
        public async Task<IActionResult> Import()
        {
            List<InvoiceExcelInput> inputs = new()
            {
                new InvoiceExcelInput() { Id = 1 , Type = nameof(InvoiceType.Standard) , DeliveredTime = DateTime.Now.AddDays(1) , Deliverer = "Ali" , DlNumber = "4887895" },
                new InvoiceExcelInput() { Id = 2 , Type = nameof(InvoiceType.Commercial) , DeliveredTime = DateTime.Now.AddDays(2) , Deliverer = "Mohammad, Head Courier" , DlNumber = "4587512" },
                new InvoiceExcelInput() { Id = 3 , Type = nameof(InvoiceType.Proforma) , DeliveredTime = DateTime.Now , Deliverer = "Akbar" , DlNumber = "999999" },
            };
            var memorystream = new MemoryStream();
            await _excelWriter.WriteAsync(inputs, memorystream);
            memorystream.Position = 0;
            return File(memorystream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Documents.xlsx");
        }
    }
}

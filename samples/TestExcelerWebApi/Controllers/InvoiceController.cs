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

        [HttpPost]
        public async Task<IActionResult> Import()
        {
            List<InvoiceExcelInput> inputs = new()
            {
                new InvoiceExcelInput() { Id = 1 , Type = "Type1" , DeliveredTime = DateTime.Now.AddDays(1) , Deliverer = "Ali" , DlNumber = "4887895" },
                new InvoiceExcelInput() { Id = 2 , Type = "Type2" , DeliveredTime = DateTime.Now.AddDays(2) , Deliverer = "Asghar" , DlNumber = "4587512" },
                new InvoiceExcelInput() { Id = 3 , Type = "Type3" , DeliveredTime = DateTime.Now , Deliverer = "Akbar" , DlNumber = "999999" },
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

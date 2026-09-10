using Exceler.Abstractions;
using Exceler.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using TestExcelerWebApi.Dtos;
using TestExcelerWebApi.Inputs;

namespace TestExcelerWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IExcelReader _excelReader;
        private readonly IExcelWriter _excelWriter;

        public DocumentsController(IExcelReader excelReader, IExcelWriter excelWriter)
        {
            _excelReader = excelReader;
            _excelWriter = excelWriter;
        }

        [HttpPost("import")]
        public IActionResult ImportDocuments(IFormFile file)
        {
            using var stream = file.OpenReadStream();

            var results = _excelReader.Read<DocumentExcelInput, DocumentDto>(stream).ToList();

            var validData = results.Where(x => x.IsValid).Select(x => x.Data).ToList();
            var invalidRows = results.Where(x => !x.IsValid).Select(x => new
            {
                Row = x.RowIndex,
                Errors = x.Errors
            }).ToList();

            return Ok(new
            {
                Message = $"Valid Data count :{validData.Count} , Invalid rows : {invalidRows.Count}",
                ValidData = validData,
                Errors = invalidRows
            });
        }
        [HttpGet("export")]
        public async Task<IActionResult> ExportDocuments()
        {
            var fakeData = new List<DocumentExcelInput>
                {
                    new DocumentExcelInput { Id = 1, Type = "Receipt", Warehouse = "inventory 1" },
                    new DocumentExcelInput { Id = 2, Type = "invoice", Warehouse = "inventory 2" }
                };
            var memorystream = new MemoryStream();
            await _excelWriter.WriteAsync(fakeData, memorystream);
            memorystream.Position = 0;
            return File(memorystream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Documents.xlsx");
        }
        [HttpGet("exportAsAsyncEnumerable")]
        public async Task ExportDocumentsAsAsyncEnumerable()
        {
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers.Add("Content-Disposition", "attachment; filename=Documents.xlsx");

            var dataStream = GenerateFakeDataStreamAsync();


            await dataStream.ToExcelAsync(_excelWriter, Response.Body, "Inventory Documents");
        }

        #region Private Methods
        private async IAsyncEnumerable<DocumentExcelInput> GenerateFakeDataStreamAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var fakeData = new List<DocumentExcelInput>
            {
                new DocumentExcelInput { Id = 1, Type = "Receipt", Warehouse = "inventory 1", Number = "REC-001", Description = "First delivery" },
                new DocumentExcelInput { Id = 2, Type = "Invoice", Warehouse = "inventory 2", Number = "INV-105", Description = "Monthly supply" },
                new DocumentExcelInput { Id = 3, Type = "Return", Warehouse = "inventory 1", Number = "RET-020", Description = "Damaged goods" }
            };

            foreach (var item in fakeData)
            {
                await Task.Delay(10, cancellationToken);

                yield return item;
            }
        }
        #endregion
    }
}

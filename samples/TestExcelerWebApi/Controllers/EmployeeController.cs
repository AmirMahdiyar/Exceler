using Exceler.Abstractions;
using Microsoft.AspNetCore.Mvc;
using TestExcelerWebApi.Dtos;
using TestExcelerWebApi.Inputs;

namespace TestExcelerWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IExcelReader _excelReader;

        public EmployeesController(IExcelReader excelReader)
        {
            _excelReader = excelReader;
        }

        [HttpPost("import")]
        public IActionResult ImportEmployees(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("File is empty");

            using var stream = file.OpenReadStream();

            var rowResults = _excelReader.Read<EmployeeExcelInput, EmployeeDto>(stream).ToList();

            var validRecords = rowResults
                .Where(x => x.IsValid)
                .Select(x => x.Data)
                .ToList();

            var invalidRecords = rowResults
                .Where(x => !x.IsValid)
                .Select(x => new
                {
                    Row = x.RowIndex,
                    Errors = x.Errors
                })
                .ToList();

            return Ok(new
            {
                TotalProcessed = rowResults.Count,
                ValidCount = validRecords.Count,
                InvalidCount = invalidRecords.Count,
                ValidData = validRecords,
                ErrorLog = invalidRecords
            });
        }
        [HttpPost("import-bulk")]
        public async Task<IActionResult> ImportBulkEmployees(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("File is empty");

            using var stream = file.OpenReadStream();

            int chunkCounter = 0;
            int totalProcessed = 0;
            var report = new List<object>();

            var chunkStream = _excelReader.ReadInChunksAsync<EmployeeExcelInput, EmployeeDto>(stream, chunkSize: 2);

            await foreach (var chunk in chunkStream)
            {
                chunkCounter++;
                totalProcessed += chunk.Count;

                var validData = chunk.Where(x => x.IsValid).Select(x => x.Data).ToList();
                var invalidRecords = chunk.Where(x => !x.IsValid).Select(x => new { Row = x.RowIndex, Errors = x.Errors }).ToList();

                report.Add(new
                {
                    ChunkNumber = chunkCounter,
                    RecordsInThisChunk = chunk.Count,
                    ValidCount = validData.Count,
                    InvalidCount = invalidRecords.Count,
                    ValidData = validData,
                    Errors = invalidRecords
                });
            }

            return Ok(new
            {
                Message = "Chunk process was successfull",
                TotalChunks = chunkCounter,
                TotalRecords = totalProcessed,
                Details = report
            });
        }
    }
}

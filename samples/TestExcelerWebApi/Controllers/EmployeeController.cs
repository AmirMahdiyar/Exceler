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
            if (file == null || file.Length == 0) return BadRequest("فایل خالی است.");

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
            if (file == null || file.Length == 0) return BadRequest("فایل خالی است.");

            using var stream = file.OpenReadStream();

            // تعریف متغیرهایی برای گزارش‌گیری و دیدن عملکرد Chunking
            int chunkCounter = 0;
            int totalProcessed = 0;
            var report = new List<object>();

            // فراخوانی متد جدید با chunkSize = 2 برای تست روی فایل کوچک
            var chunkStream = _excelReader.ReadInChunksAsync<EmployeeExcelInput, EmployeeDto>(stream, chunkSize: 2);

            // حلقه نامزامون روی دسته‌های آماده شده
            await foreach (var chunk in chunkStream)
            {
                chunkCounter++;
                totalProcessed += chunk.Count;

                var validData = chunk.Where(x => x.IsValid).Select(x => x.Data).ToList();
                var invalidRecords = chunk.Where(x => !x.IsValid).Select(x => new { Row = x.RowIndex, Errors = x.Errors }).ToList();

                // در دنیای واقعی اینجا دستور SqlBulkCopy نوشته میشه
                // await _dbContext.BulkInsertAsync(validData);

                // ما اینجا فقط لاگ می‌کنیم تا رفتار فریم‌ورک رو ببینیم
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
                Message = "پردازش دسته‌ای (Chunking) با موفقیت به پایان رسید.",
                TotalChunks = chunkCounter,
                TotalRecords = totalProcessed,
                Details = report
            });
        }
    }
}

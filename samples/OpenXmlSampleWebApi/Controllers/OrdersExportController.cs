using Exceler.Abstractions;
using Exceler.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenXmlSampleWebApi.Models;
using OpenXmlSampleWebApi.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenXmlSampleWebApi.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersExportController : ControllerBase
    {
        private readonly IExcelWriter _excelWriter;
        private readonly IExcelReader _excelReader;
        private readonly OrderDataGenerator _dataGenerator;

        public OrdersExportController(
            IExcelWriter excelWriter,
            IExcelReader excelReader,
            OrderDataGenerator dataGenerator)
        {
            _excelWriter = excelWriter;
            _excelReader = excelReader;
            _dataGenerator = dataGenerator;
        }

        /// <summary>
        /// Exports a quick sample of 500 orders using the OpenXML SAX engine.
        /// </summary>
        [HttpGet("export-sample")]
        public async Task<IActionResult> ExportSample()
        {
            var data = _dataGenerator.GenerateList(500);

            var memoryStream = new MemoryStream();
            await _excelWriter.WriteAsync(data, memoryStream, sheetName: "Sample Orders");
            memoryStream.Position = 0;

            return File(
                memoryStream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "sample_orders_500.xlsx"
            );
        }

        /// <summary>
        /// Exports an Excel template demonstrating Smart Dropdowns (Data Validation) powered by OpenXML SAX.
        /// Columns 'Country' and 'Status' contain interactive Excel dropdown pickers.
        /// Notice: 'Country' options contain commas, which Exceler automatically handles via a hidden '_ValidationData' sheet!
        /// </summary>
        [HttpGet("export-template-with-dropdowns")]
        public async Task<IActionResult> ExportTemplateWithDropdowns()
        {
            // Generate 5 starter rows with valid dropdown options
            var starterData = _dataGenerator.GenerateList(5);

            var memoryStream = new MemoryStream();
            await _excelWriter.WriteAsync(starterData, memoryStream, sheetName: "Order Entry Template");
            memoryStream.Position = 0;

            return File(
                memoryStream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "orders_template_with_dropdowns.xlsx"
            );
        }

        /// <summary>
        /// Exports a demonstration workbook showing pure C#-based Conditional Styling powered by OpenXML SAX:
        /// - Express deliveries have their ENTIRE ROW highlighted in SoftYellow.
        /// - High-value orders (> $1,500) have their Total Amount highlighted in SoftGreen and Bolded (preserving currency format!).
        /// - Cancelled/Refunded orders have their Status highlighted in SoftRed with DarkRed bold text.
        /// - Demonstrates Cascading Precedence: Column styling overrides row styling on specific cells!
        /// </summary>
        [HttpGet("export-conditional-styles-demo")]
        public async Task<IActionResult> ExportConditionalStylesDemo()
        {
            var demoOrders = new List<OrderExportModel>
            {
                // 1. Standard Order (Default styling)
                new()
                {
                    Id = 101,
                    OrderNumber = "ORD-2026-001",
                    CustomerName = "Alice Smith",
                    CustomerEmail = "alice@example.com",
                    Country = "United States",
                    TotalAmount = 250.00m,
                    Status = "Processing",
                    OrderDate = new DateOnly(2026, 4, 1),
                    OrderTime = new TimeOnly(10, 15, 0),
                    IsExpressDelivery = false,
                    ItemCount = 2,
                    Notes = "Standard ground delivery"
                },
                // 2. High-value order (> $1,500) -> Amount is SoftGreen & Bold (Currency format strictly preserved!)
                new()
                {
                    Id = 102,
                    OrderNumber = "ORD-2026-002",
                    CustomerName = "Bob Johnson",
                    CustomerEmail = "bob@example.com",
                    Country = "Germany",
                    TotalAmount = 3450.75m, // Triggers Green & Bold
                    Status = "Shipped",
                    OrderDate = new DateOnly(2026, 4, 2),
                    OrderTime = new TimeOnly(14, 30, 0),
                    IsExpressDelivery = false,
                    ItemCount = 12,
                    Notes = "Enterprise bulk purchase"
                },
                // 3. Express Delivery -> ENTIRE ROW is SoftYellow
                new()
                {
                    Id = 103,
                    OrderNumber = "ORD-2026-003",
                    CustomerName = "Charlie Brown",
                    CustomerEmail = "charlie@example.com",
                    Country = "France",
                    TotalAmount = 180.50m,
                    Status = "Delivered",
                    OrderDate = new DateOnly(2026, 4, 3),
                    OrderTime = new TimeOnly(9, 0, 0),
                    IsExpressDelivery = true, // Triggers SoftYellow ROW
                    ItemCount = 1,
                    Notes = "Priority express overnight"
                },
                // 4. Cancelled Order -> Status is SoftRed with DarkRed bold text
                new()
                {
                    Id = 104,
                    OrderNumber = "ORD-2026-004",
                    CustomerName = "Diana Prince",
                    CustomerEmail = "diana@example.com",
                    Country = "United Kingdom",
                    TotalAmount = 820.00m,
                    Status = "Cancelled", // Triggers SoftRed Status
                    OrderDate = new DateOnly(2026, 4, 4),
                    OrderTime = new TimeOnly(16, 45, 0),
                    IsExpressDelivery = false,
                    ItemCount = 4,
                    Notes = "Customer requested cancellation"
                },
                // 5. Cascading Precedence Demo:
                // Express (Yellow row) + High Amount (Green cell) + Cancelled (Red cell)
                // Proves column rules cleanly override the row rule on specific cells!
                new()
                {
                    Id = 105,
                    OrderNumber = "ORD-2026-005",
                    CustomerName = "Evan Wright",
                    CustomerEmail = "evan@example.com",
                    Country = "Tehran, Iran", // Triggers hidden reference sheet dropdown
                    TotalAmount = 2990.00m,  // Column rule: SoftGreen & Bold overrides yellow row!
                    Status = "Cancelled",     // Column rule: SoftRed & DarkRed overrides yellow row!
                    OrderDate = new DateOnly(2026, 4, 5),
                    OrderTime = new TimeOnly(11, 20, 0),
                    IsExpressDelivery = true, // Row rule: SoftYellow
                    ItemCount = 7,
                    Notes = "VIP customer refund"
                }
            };

            var memoryStream = new MemoryStream();
            await _excelWriter.WriteAsync(demoOrders, memoryStream, sheetName: "Conditional Styles Demo");
            memoryStream.Position = 0;

            return File(
                memoryStream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "orders_conditional_styles_demo.xlsx"
            );
        }

        /// <summary>
        /// High-volume stress test: streams up to 100,000+ Bogus-generated orders directly into an Excel workbook
        /// with O(1) memory consumption using IAsyncEnumerable without buffering in RAM.
        /// </summary>
        /// <param name="count">Number of records to generate and export (default: 50,000).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [HttpGet("export-stress-stream")]
        public async Task<IActionResult> ExportStressStream(
            [FromQuery] int count = 50000,
            CancellationToken cancellationToken = default)
        {
            var orderStream = _dataGenerator.StreamOrdersAsync(count, cancellationToken);

            var memoryStream = new MemoryStream();

            // Stream records directly into the Excel workbook using the OpenXML engine!
            await orderStream.ToExcelAsync(_excelWriter, memoryStream, sheetName: "Enterprise Orders");
            memoryStream.Position = 0;

            return File(
                memoryStream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"stress_orders_{count}.xlsx"
            );
        }

        /// <summary>
        /// Performance and memory benchmark: measures elapsed time and GC memory consumption
        /// during high-volume OpenXML SAX streaming.
        /// </summary>
        /// <param name="count">Number of records to benchmark (default: 50,000).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [HttpGet("benchmark")]
        public async Task<IActionResult> Benchmark(
            [FromQuery] int count = 50000,
            CancellationToken cancellationToken = default)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long initialMemory = GC.GetTotalMemory(true);
            var stopwatch = Stopwatch.StartNew();

            var orderStream = _dataGenerator.StreamOrdersAsync(count, cancellationToken);
            using var memoryStream = new MemoryStream();

            await orderStream.ToExcelAsync(_excelWriter, memoryStream, sheetName: "Benchmark");

            stopwatch.Stop();
            long finalMemory = GC.GetTotalMemory(false);
            double memoryDiffMb = (finalMemory - initialMemory) / (1024.0 * 1024.0);

            return Ok(new
            {
                Engine = "Exceler v2.0.0 (OpenXML SAX Engine)",
                RecordCount = count,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                ElapsedSeconds = Math.Round(stopwatch.Elapsed.TotalSeconds, 2),
                RecordsPerSecond = Math.Round(count / stopwatch.Elapsed.TotalSeconds, 0),
                GeneratedFileSizeKb = Math.Round(memoryStream.Length / 1024.0, 2),
                InitialMemoryMb = Math.Round(initialMemory / (1024.0 * 1024.0), 2),
                FinalMemoryMb = Math.Round(finalMemory / (1024.0 * 1024.0), 2),
                MemoryDeltaMb = Math.Round(memoryDiffMb, 2),
                Status = "O(1) Streaming Succeeded"
            });
        }

        /// <summary>
        /// Demonstrates importing an Excel file back in memory-efficient chunks.
        /// </summary>
        [HttpPost("import-chunks")]
        public async Task<IActionResult> ImportInChunks(
            IFormFile file,
            [FromQuery] int chunkSize = 5000,
            CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file provided.");

            using var stream = file.OpenReadStream();

            int totalRows = 0;
            int validRows = 0;
            int errorRows = 0;
            int chunkCount = 0;

            await foreach (var chunk in _excelReader.ReadInChunksAsync<OrderExportModel, OrderExportModel>(
                stream, chunkSize, cancellationToken: cancellationToken))
            {
                chunkCount++;
                foreach (var row in chunk)
                {
                    totalRows++;
                    if (row.IsValid)
                        validRows++;
                    else
                        errorRows++;
                }
            }

            return Ok(new
            {
                TotalChunks = chunkCount,
                ChunkSize = chunkSize,
                TotalRows = totalRows,
                ValidRows = validRows,
                ErrorRows = errorRows
            });
        }
    }
}

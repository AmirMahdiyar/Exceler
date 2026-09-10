using Exceler.Abstractions;
using Exceler.Configuration;
using Exceler.Core;
using Exceler.Tests.Common.Fixtures;
using FluentAssertions;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Exceler.Tests.Unit.Pipeline
{
    public class PipelineHandlersEdgeTests
    {
        private class TimeAndStyleItem
        {
            public TimeOnly ShiftTime { get; set; }
            public string Status { get; set; } = string.Empty;
        }

        private class TimeAndStyleProfile : ExcelProfile<TimeAndStyleItem>
        {
            public TimeAndStyleProfile()
            {
                Map(x => x.ShiftTime)
                    .ToColumn(1)
                    .WithHeader("Shift");

                Map(x => x.Status)
                    .ToColumn(2)
                    .WithHeader("Status")
                    .WithFontColor(ExcelColor.DarkBlue)
                    .WithBackgroundColor(ExcelColor.SoftYellow);
            }
        }

        [Fact]
        public async Task TimeOnly_property_is_formatted_as_time_in_generated_spreadsheet()
        {
            var sut = ExcelerTestBed.CreateWriter<TimeAndStyleItem, TimeAndStyleProfile>();

            var data = new List<TimeAndStyleItem>
            {
                new() { ShiftTime = new TimeOnly(14, 30, 0), Status = "Active" }
            };

            var bytes = await sut.Write(data);

            using var stream = new MemoryStream(bytes);
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            Convert.ToDouble(sheet.Cells[2, 1].Value).Should().BeApproximately(14.5 / 24.0, 0.0001);
            sheet.Cells[2, 1].Style.Numberformat.Format.Should().Be("hh:mm:ss");
        }

        [Fact]
        public async Task Font_and_background_colors_are_applied_to_styled_cells()
        {
            var sut = ExcelerTestBed.CreateWriter<TimeAndStyleItem, TimeAndStyleProfile>();

            var data = new List<TimeAndStyleItem>
            {
                new() { ShiftTime = new TimeOnly(9, 0, 0), Status = "VIP" }
            };

            var bytes = await sut.Write(data);

            using var stream = new MemoryStream(bytes);
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            sheet.Cells[2, 2].Style.Font.Color.Rgb.Should().NotBeNullOrEmpty();
            sheet.Cells[2, 2].Style.Fill.BackgroundColor.Rgb.Should().NotBeNullOrEmpty();
        }

        private class SyncValidationItem
        {
            public int Score { get; set; }
        }

        private class SyncValidationProfile : ExcelProfile<SyncValidationItem>
        {
            public SyncValidationProfile()
            {
                Map(x => x.Score).ToColumn(1).WithHeader("Score");
            }
        }

        private class SyncItemValidator : IExcelValidator<SyncValidationItem>
        {
            public IEnumerable<string> Validate(SyncValidationItem input)
            {
                if (input.Score < 50)
                {
                    yield return "Score must be at least 50";
                }
            }
        }

        [Fact]
        public async Task Synchronous_validator_is_invoked_during_asynchronous_chunk_reading()
        {
            var sut = ExcelerTestBed.Configure()
                .WithProfile<SyncValidationItem, SyncValidationProfile>()
                .WithValidator<SyncValidationItem, SyncItemValidator>()
                .BuildReader();

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Sheet1");
            ws.Cells[1, 1].Value = "Score";
            ws.Cells[2, 1].Value = 20; // Invalid, < 50
            using var stream = new MemoryStream(package.GetAsByteArray());

            var results = new List<ExcelRowResult<SyncValidationItem>>();
            await foreach (var chunk in sut.ReadInChunksAsync<SyncValidationItem, SyncValidationItem>(stream))
            {
                results.AddRange(chunk);
            }

            results.Should().HaveCount(1);
            results[0].IsValid.Should().BeFalse();
            results[0].Errors.Should().Contain("Score must be at least 50");
        }

        private class CancellableItem
        {
            public string Name { get; set; } = string.Empty;
        }

        private class CancellableProfile : ExcelProfile<CancellableItem>
        {
            public CancellableProfile()
            {
                Map(x => x.Name).ToColumn(1).WithHeader("Name");
            }
        }

        private class SlowAsyncProcessor : IAsyncExcelProcessor<CancellableItem, CancellableItem>
        {
            public async Task<CancellableItem> ProcessAsync(CancellableItem input, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
                return input;
            }
        }

        [Fact]
        public async Task Asynchronous_chunk_reading_aborts_when_cancellation_is_requested()
        {
            var sut = ExcelerTestBed.Configure()
                .WithProfile<CancellableItem, CancellableProfile>()
                .WithAsyncProcessor<CancellableItem, CancellableItem, SlowAsyncProcessor>()
                .BuildReader();

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Sheet1");
            ws.Cells[1, 1].Value = "Name";
            ws.Cells[2, 1].Value = "Test";
            using var stream = new MemoryStream(package.GetAsByteArray());

            using var cts = new CancellationTokenSource();
            cts.Cancel(); // Pre-cancelled

            var act = async () =>
            {
                await foreach (var _ in sut.ReadInChunksAsync<CancellableItem, CancellableItem>(stream, cancellationToken: cts.Token))
                {
                }
            };

            await act.Should().ThrowAsync<OperationCanceledException>();
        }
    }
}

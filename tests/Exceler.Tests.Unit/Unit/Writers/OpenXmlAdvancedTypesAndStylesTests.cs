using Exceler.Abstractions;
using Exceler.Configuration;
using Exceler.Tests.Common.Fixtures;
using FluentAssertions;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Exceler.Tests.Unit.Writers
{
    public enum PriorityLevel
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }

    public class AdvancedTypesModel
    {
        public int Id { get; set; }
        public DateTime DateTimeVal { get; set; }
        public DateTimeOffset DateTimeOffsetVal { get; set; }
        public TimeSpan TimeSpanVal { get; set; }
        public float FloatVal { get; set; }
        public double DoubleVal { get; set; }
        public long LongVal { get; set; }
        public short ShortVal { get; set; }
        public byte ByteVal { get; set; }
        public Guid GuidVal { get; set; }
        public PriorityLevel Priority { get; set; }
        public string? StyledNullVal { get; set; }
    }

    public class AdvancedTypesProfile : ExcelProfile<AdvancedTypesModel>
    {
        public AdvancedTypesProfile()
        {
            Map(x => x.Id).ToColumn(1).WithHeader("ID");
            Map(x => x.DateTimeVal).ToColumn(2).WithHeader("DateTime").WithFormat("m/d/yy h:mm");
            Map(x => x.DateTimeOffsetVal).ToColumn(3).WithHeader("DateTimeOffset");
            Map(x => x.TimeSpanVal).ToColumn(4).WithHeader("TimeSpan").WithFormat("h:mm:ss");
            Map(x => x.FloatVal).ToColumn(5).WithHeader("Float").WithFormat("0.00");
            Map(x => x.DoubleVal).ToColumn(6).WithHeader("Double").WithFormat("#,##0.00");
            Map(x => x.LongVal).ToColumn(7).WithHeader("Long").WithFormat("0");
            Map(x => x.ShortVal).ToColumn(8).WithHeader("Short");
            Map(x => x.ByteVal).ToColumn(9).WithHeader("Byte");
            Map(x => x.GuidVal).ToColumn(10).WithHeader("Guid");
            Map(x => x.Priority).ToColumn(11).WithHeader("Priority").WithFontColor("#0000FF"); // Font color only (no bold)
            Map(x => x.StyledNullVal).ToColumn(12).WithHeader("StyledNull")
                .WithBackgroundColor("#FFFF00") // Background fill on null cell
                .WithFormat("0.00"); // Reusing standard format
        }
    }

    public class DuplicateCustomFormatModel
    {
        public decimal Amount1 { get; set; }
        public decimal Amount2 { get; set; }
    }

    public class DuplicateCustomFormatProfile : ExcelProfile<DuplicateCustomFormatModel>
    {
        public DuplicateCustomFormatProfile()
        {
            // Both columns use identical custom format code to test format deduplication
            Map(x => x.Amount1).ToColumn(1).WithHeader("Amt1").WithFormat("[$$-409]#,##0.00;[Red]-[$$-409]#,##0.00");
            Map(x => x.Amount2).ToColumn(2).WithHeader("Amt2").WithFormat("[$$-409]#,##0.00;[Red]-[$$-409]#,##0.00");
        }
    }

    public class OpenXmlAdvancedTypesAndStylesTests : ExcelerTestBase
    {
        [Fact]
        public async Task OpenXml_exports_all_numeric_temporal_and_object_types_accurately()
        {
            // Arrange
            var writer = ExcelerTestBed.CreateWriter<AdvancedTypesModel, AdvancedTypesProfile>(ExcelerEngine.OpenXml);
            var guid = Guid.NewGuid();
            var items = new List<AdvancedTypesModel>
            {
                new()
                {
                    Id = 1,
                    DateTimeVal = new DateTime(2026, 6, 15, 10, 30, 0),
                    DateTimeOffsetVal = new DateTimeOffset(2026, 6, 15, 10, 30, 0, TimeSpan.Zero),
                    TimeSpanVal = new TimeSpan(14, 45, 30),
                    FloatVal = 12.34f,
                    DoubleVal = 98765.4321,
                    LongVal = 9876543210123L,
                    ShortVal = 32000,
                    ByteVal = 255,
                    GuidVal = guid,
                    Priority = PriorityLevel.Critical,
                    StyledNullVal = null // Will be exported as an empty cell with background fill
                }
            };

            // Act
            byte[] bytes = await writer.Write(items);

            // Assert
            using var stream = new MemoryStream(bytes);
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            sheet.Cells[2, 1].Value.Should().Be(1.0);
            Convert.ToDouble(sheet.Cells[2, 5].Value).Should().BeApproximately(12.34, 0.01);
            Convert.ToDouble(sheet.Cells[2, 6].Value).Should().BeApproximately(98765.4321, 0.0001);
            Convert.ToInt64(sheet.Cells[2, 7].Value).Should().Be(9876543210123L);
            sheet.Cells[2, 8].Value.Should().Be(32000.0);
            sheet.Cells[2, 9].Value.Should().Be(255.0);
            sheet.Cells[2, 10].Text.Should().Be(guid.ToString());
            sheet.Cells[2, 11].Text.Should().Be("Critical");
            sheet.Cells[2, 12].Value.Should().BeNull(); // Null value preserved with styling
        }

        [Fact]
        public async Task OpenXml_deduplicates_identical_custom_number_formats_in_stylesheet()
        {
            // Arrange
            var writer = ExcelerTestBed.CreateWriter<DuplicateCustomFormatModel, DuplicateCustomFormatProfile>(ExcelerEngine.OpenXml);
            var items = new List<DuplicateCustomFormatModel>
            {
                new() { Amount1 = 1200.50m, Amount2 = 3400.75m }
            };

            // Act
            byte[] bytes = await writer.Write(items);

            // Assert
            using var stream = new MemoryStream(bytes);
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            sheet.Cells[2, 1].Value.Should().Be(1200.50);
            sheet.Cells[2, 2].Value.Should().Be(3400.75);
        }
    }
}

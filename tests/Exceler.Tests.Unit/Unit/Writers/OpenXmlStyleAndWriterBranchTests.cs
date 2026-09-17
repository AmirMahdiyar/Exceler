using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Exceler.Abstractions;
using Exceler.Configuration;
using Exceler.Core.OpenXml;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Exceler.Tests.Unit.Unit.Writers
{
    public class OpenXmlStyleAndWriterBranchTests
    {
        private class BranchTestModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public decimal Amount1 { get; set; }
            public decimal Amount2 { get; set; }
            public double Ratio { get; set; }
            public DateOnly DateVal { get; set; }
            public TimeOnly TimeVal { get; set; }
        }

        private class BranchTestProfile : ExcelProfile<BranchTestModel>
        {
            public BranchTestProfile()
            {
                Map(x => x.Id).ToColumn(1).WithFormat("0");
                Map(x => x.Name).ToColumn(2).WithFormat("@");
                // Custom format used twice to trigger "existing != null" branch in OpenXmlStyleManager
                Map(x => x.Amount1).ToColumn(3).WithFormat("[$$-en-US] #,##0.00");
                Map(x => x.Amount2).ToColumn(4).WithFormat("[$$-en-US] #,##0.00");
                Map(x => x.Ratio).ToColumn(5).WithFormat("0.00%");
                // Inferred formats for DateOnly and TimeOnly
                Map(x => x.DateVal).ToColumn(6);
                Map(x => x.TimeVal).ToColumn(7);
            }
        }

        [Fact]
        public void OpenXmlStyleManager_BranchCoverage_Tests()
        {
            var profile = new BranchTestProfile();
            profile.EnsureBuilt();

            var manager = OpenXmlStyleManager.Create(profile);

            // 1. GetColumnStyleIndex: mapped column returns > 0, unmapped column returns 0
            manager.GetColumnStyleIndex(1).Should().BeGreaterThan(0U);
            manager.GetColumnStyleIndex(999).Should().Be(0U);

            // 2. GetColumnNumberFormat: mapped returns format, unmapped returns null
            manager.GetColumnNumberFormat(1).Should().Be("0");
            manager.GetColumnNumberFormat(6).Should().Be("yyyy-mm-dd"); // inferred
            manager.GetColumnNumberFormat(7).Should().Be("hh:mm:ss"); // inferred
            manager.GetColumnNumberFormat(999).Should().BeNull();

            // 3. HeaderStyleIndex
            manager.HeaderStyleIndex.Should().Be(1U);
            manager.Stylesheet.Should().NotBeNull();
        }

        [Fact]
        public void OpenXmlWriterEngine_NullServiceProvider_ThrowsArgumentNullException()
        {
            Action act = () => new OpenXmlWriterEngine(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void OpenXmlCellWriter_Branches_Tests()
        {
            using var ms = new MemoryStream();
            using var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook);
            var wb = doc.AddWorkbookPart();
            var ws = wb.AddNewPart<WorksheetPart>();

            using (var writer = OpenXmlWriter.Create(ws))
            {
                writer.WriteStartElement(new Worksheet());
                writer.WriteStartElement(new SheetData());
                writer.WriteStartElement(new Row { RowIndex = 1 });

                // Test WriteStringCell with null value (covers value ?? string.Empty branch)
                OpenXmlCellWriter.WriteStringCell(writer, "A1", null, 0);

                // Test WriteEmptyCell with styleIndex = 0 (no-op) and styleIndex > 0
                OpenXmlCellWriter.WriteEmptyCell(writer, "B1", 0);
                OpenXmlCellWriter.WriteEmptyCell(writer, "C1", 1);

                // Test WriteCell with DBNull.Value (covers DBNull branch)
                OpenXmlCellWriter.WriteCell(writer, "D1", DBNull.Value, 0);
                OpenXmlCellWriter.WriteCell(writer, "E1", DBNull.Value, 1);

                // Test WriteCell with null
                OpenXmlCellWriter.WriteCell(writer, "F1", null, 0);

                writer.WriteEndElement(); // Row
                writer.WriteEndElement(); // SheetData
                writer.WriteEndElement(); // Worksheet
            }
        }
    }
}

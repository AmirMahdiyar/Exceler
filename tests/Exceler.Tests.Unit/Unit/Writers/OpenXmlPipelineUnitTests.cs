using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Exceler.Configuration;
using Exceler.Core.OpenXml;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Exceler.Tests.Unit.Unit.Writers
{
    public class OpenXmlPipelineUnitTests
    {
        private class DummyModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        private class DummyProfileWithHeaders : ExcelProfile<DummyModel>
        {
            public DummyProfileWithHeaders()
            {
                Map(x => x.Id).ToColumn(1).WithHeader("ID");
                Map(x => x.Name).ToColumn(2).WithHeader("Name");
            }
        }

        private class DummyProfileNoHeaders : ExcelProfile<DummyModel>
        {
            public DummyProfileNoHeaders()
            {
                Map(x => x.Id).ToColumn(1);
                Map(x => x.Name).ToColumn(2);
            }
        }

        #region OpenXmlPackageBuilder Tests

        [Fact]
        public void PackageBuilder_NullStream_ThrowsArgumentNullException()
        {
            Action act = () => new OpenXmlPackageBuilder(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void PackageBuilder_SetStylesheet_NullStylesheet_ThrowsArgumentNullException()
        {
            using var ms = new MemoryStream();
            using var builder = new OpenXmlPackageBuilder(ms);
            Action act = () => builder.SetStylesheet(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void PackageBuilder_RegisterSheet_NullPart_ThrowsArgumentNullException()
        {
            using var ms = new MemoryStream();
            using var builder = new OpenXmlPackageBuilder(ms);
            Action act = () => builder.RegisterSheet(null!, "Sheet1");
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void PackageBuilder_BuildAndSave_CreatesValidPackage()
        {
            using var ms = new MemoryStream();
            using (var builder = new OpenXmlPackageBuilder(ms))
            {
                builder.SetStylesheet(new Stylesheet());
                var part1 = builder.CreateWorksheetPart();
                builder.RegisterSheet(part1, "MySheet");

                var part2 = builder.CreateWorksheetPart();
                builder.RegisterSheet(part2, "_HiddenRef", SheetStateValues.Hidden);

                builder.Save();
            }

            ms.Length.Should().BeGreaterThan(0);

            // Re-open and verify sheets
            using var doc = SpreadsheetDocument.Open(ms, false);
            doc.WorkbookPart.Should().NotBeNull();
            var sheets = doc.WorkbookPart!.Workbook.Sheets!.Elements<Sheet>().ToList();
            sheets.Should().HaveCount(2);
            sheets[0].Name!.Value.Should().Be("MySheet");
            sheets[1].Name!.Value.Should().Be("_HiddenRef");
            sheets[1].State!.Value.Should().Be(SheetStateValues.Hidden);
        }

        #endregion

        #region OpenXmlValidationReferenceSheetWriter Tests

        [Fact]
        public void ReferenceSheetWriter_NullGuards()
        {
            using var ms = new MemoryStream();
            using var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook);
            var wb = doc.AddWorkbookPart();
            var part = wb.AddNewPart<WorksheetPart>();

            var list = new List<KeyValuePair<int, ColumnStyle>>();
            var dict = new Dictionary<int, string>();

            Action act1 = () => OpenXmlValidationReferenceSheetWriter.WriteReferenceSheet(null!, list, dict);
            Action act2 = () => OpenXmlValidationReferenceSheetWriter.WriteReferenceSheet(part, null!, dict);
            Action act3 = () => OpenXmlValidationReferenceSheetWriter.WriteReferenceSheet(part, list, null!);

            act1.Should().Throw<ArgumentNullException>();
            act2.Should().Throw<ArgumentNullException>();
            act3.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ReferenceSheetWriter_EmptyList_ReturnsWithoutWriting()
        {
            using var ms = new MemoryStream();
            using var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook);
            var wb = doc.AddWorkbookPart();
            var part = wb.AddNewPart<WorksheetPart>();

            var list = new List<KeyValuePair<int, ColumnStyle>>();
            var dict = new Dictionary<int, string>();

            OpenXmlValidationReferenceSheetWriter.WriteReferenceSheet(part, list, dict);
            dict.Should().BeEmpty();
        }

        [Fact]
        public void ReferenceSheetWriter_WritesColumnsAndPopulatesRanges()
        {
            using var ms = new MemoryStream();
            using var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook);
            var wb = doc.AddWorkbookPart();
            var part = wb.AddNewPart<WorksheetPart>();

            var col1Style = new ColumnStyle();
            col1Style.Dropdown = new DropdownConfiguration(new[] { "Opt1", "Opt2", "Opt3" });

            var col2Style = new ColumnStyle();
            col2Style.Dropdown = new DropdownConfiguration(new[] { "ValA", "ValB" });

            var list = new List<KeyValuePair<int, ColumnStyle>>
            {
                new(2, col1Style),
                new(5, col2Style)
            };
            var refRanges = new Dictionary<int, string>();

            OpenXmlValidationReferenceSheetWriter.WriteReferenceSheet(part, list, refRanges);

            refRanges.Should().ContainKey(2);
            refRanges[2].Should().Be("'_ValidationData'!$A$1:$A$3");
            refRanges.Should().ContainKey(5);
            refRanges[5].Should().Be("'_ValidationData'!$B$1:$B$2");
        }

        #endregion

        #region OpenXmlRowStreamer Tests

        [Fact]
        public void RowStreamer_NullGuards()
        {
            var profile = new DummyProfileWithHeaders();
            profile.EnsureBuilt();
            var manager = OpenXmlStyleManager.Create(profile);

            Action act1 = () => new OpenXmlRowStreamer<DummyModel>(null!, manager);
            Action act2 = () => new OpenXmlRowStreamer<DummyModel>(profile, null!);

            act1.Should().Throw<ArgumentNullException>();
            act2.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void RowStreamer_NoHeadersProfile_WriteHeaders_ReturnsUnchangedRow()
        {
            var profile = new DummyProfileNoHeaders();
            profile.EnsureBuilt();
            var manager = OpenXmlStyleManager.Create(profile);
            var streamer = new OpenXmlRowStreamer<DummyModel>(profile, manager);

            using var ms = new MemoryStream();
            using var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook);
            var wb = doc.AddWorkbookPart();
            var ws = wb.AddNewPart<WorksheetPart>();

            using (var writer = OpenXmlWriter.Create(ws))
            {
                writer.WriteStartElement(new Worksheet());
                writer.WriteStartElement(new SheetData());

                uint nextRow = streamer.WriteHeaders(writer, 1);
                nextRow.Should().Be(1);

                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        [Fact]
        public void RowStreamer_NullData_ReturnsStartRowUnchanged()
        {
            var profile = new DummyProfileWithHeaders();
            profile.EnsureBuilt();
            var manager = OpenXmlStyleManager.Create(profile);
            var streamer = new OpenXmlRowStreamer<DummyModel>(profile, manager);

            using var ms = new MemoryStream();
            using var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook);
            var wb = doc.AddWorkbookPart();
            var ws = wb.AddNewPart<WorksheetPart>();

            using (var writer = OpenXmlWriter.Create(ws))
            {
                writer.WriteStartElement(new Worksheet());
                writer.WriteStartElement(new SheetData());

                uint nextRow = streamer.WriteRows(writer, 5, null);
                nextRow.Should().Be(5);

                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        [Fact]
        public async Task RowStreamer_NullAsyncData_ReturnsStartRowUnchanged()
        {
            var profile = new DummyProfileWithHeaders();
            profile.EnsureBuilt();
            var manager = OpenXmlStyleManager.Create(profile);
            var streamer = new OpenXmlRowStreamer<DummyModel>(profile, manager);

            using var ms = new MemoryStream();
            using var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook);
            var wb = doc.AddWorkbookPart();
            var ws = wb.AddNewPart<WorksheetPart>();

            using (var writer = OpenXmlWriter.Create(ws))
            {
                writer.WriteStartElement(new Worksheet());
                writer.WriteStartElement(new SheetData());

                uint nextRow = await streamer.WriteRowsAsync(writer, 10, null);
                nextRow.Should().Be(10);

                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        [Fact]
        public void RowStreamer_DataWithNullElements_SkipsNullElements()
        {
            var profile = new DummyProfileWithHeaders();
            profile.EnsureBuilt();
            var manager = OpenXmlStyleManager.Create(profile);
            var streamer = new OpenXmlRowStreamer<DummyModel>(profile, manager);

            using var ms = new MemoryStream();
            using var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook);
            var wb = doc.AddWorkbookPart();
            var ws = wb.AddNewPart<WorksheetPart>();

            var data = new List<DummyModel?> { null, new DummyModel { Id = 1, Name = "Item 1" }, null };

            using (var writer = OpenXmlWriter.Create(ws))
            {
                writer.WriteStartElement(new Worksheet());
                writer.WriteStartElement(new SheetData());

                uint nextRow = streamer.WriteRows(writer, 2, data!);
                nextRow.Should().Be(3); // Only 1 non-null element streamed

                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        [Fact]
        public async Task RowStreamer_AsyncDataWithNullElements_SkipsNullElements()
        {
            var profile = new DummyProfileWithHeaders();
            profile.EnsureBuilt();
            var manager = OpenXmlStyleManager.Create(profile);
            var streamer = new OpenXmlRowStreamer<DummyModel>(profile, manager);

            using var ms = new MemoryStream();
            using var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook);
            var wb = doc.AddWorkbookPart();
            var ws = wb.AddNewPart<WorksheetPart>();

            async IAsyncEnumerable<DummyModel?> GetStream()
            {
                yield return null;
                yield return new DummyModel { Id = 10, Name = "Async 10" };
                yield return null;
                await Task.CompletedTask;
            }

            using (var writer = OpenXmlWriter.Create(ws))
            {
                writer.WriteStartElement(new Worksheet());
                writer.WriteStartElement(new SheetData());

                uint nextRow = await streamer.WriteRowsAsync(writer, 4, GetStream()!);
                nextRow.Should().Be(5); // Only 1 non-null element streamed

                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        #endregion

        #region OpenXmlWorksheetCoordinator Tests

        [Fact]
        public async Task WorksheetCoordinator_NullGuards()
        {
            var profile = new DummyProfileWithHeaders();
            profile.EnsureBuilt();
            var manager = OpenXmlStyleManager.Create(profile);
            var streamer = new OpenXmlRowStreamer<DummyModel>(profile, manager);

            using var ms = new MemoryStream();
            using var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook);
            var wb = doc.AddWorkbookPart();
            var ws = wb.AddNewPart<WorksheetPart>();

            Func<Task> act1 = () => OpenXmlWorksheetCoordinator.WriteWorksheetAsync<DummyModel>(null!, profile, streamer, null, null, null);
            Func<Task> act2 = () => OpenXmlWorksheetCoordinator.WriteWorksheetAsync<DummyModel>(ws, null!, streamer, null, null, null);
            Func<Task> act3 = () => OpenXmlWorksheetCoordinator.WriteWorksheetAsync<DummyModel>(ws, profile, null!, null, null, null);

            await act1.Should().ThrowAsync<ArgumentNullException>();
            await act2.Should().ThrowAsync<ArgumentNullException>();
            await act3.Should().ThrowAsync<ArgumentNullException>();
        }

        #endregion
    }
}

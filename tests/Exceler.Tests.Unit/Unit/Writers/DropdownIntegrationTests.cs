using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Exceler.Abstractions;
using Exceler.Configuration;
using Exceler.Core.EPPlus;
using Exceler.Core.OpenXml;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Exceler.Tests.Unit.Unit.Writers
{
    public class DropdownIntegrationTests
    {
        public class DropdownItem
        {
            public string Status { get; set; } = string.Empty;
            public string Branch { get; set; } = string.Empty;
        }

        public class InlineDropdownProfile : ExcelProfile<DropdownItem>
        {
            public InlineDropdownProfile()
            {
                Map(x => x.Status)
                    .ToColumn(1)
                    .WithHeader("Status")
                    .WithDropdown("Active", "Inactive", "Suspended");
            }
        }

        public class ReferenceDropdownProfile : ExcelProfile<DropdownItem>
        {
            public ReferenceDropdownProfile()
            {
                Map(x => x.Status)
                    .ToColumn(1)
                    .WithHeader("Status")
                    .WithDropdown("Active", "Inactive");

                // Branch contains commas, forcing reference sheet routing
                Map(x => x.Branch)
                    .ToColumn(2)
                    .WithHeader("Branch")
                    .WithDropdown(new[] { "Tehran, Center", "Isfahan, North", "Shiraz, South" }, cfg =>
                    {
                        cfg.ErrorTitle = "Branch Error";
                        cfg.ErrorMessage = "Please choose a valid branch.";
                        cfg.ShowInputMessage = true;
                        cfg.InputTitle = "Branch Selection";
                        cfg.InputMessage = "Select branch from the list.";
                    });
            }
        }

        [Fact]
        public async Task OpenXml_InlineDropdown_GeneratesValidDataValidation()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ExcelProfile<DropdownItem>>(new InlineDropdownProfile());
            var provider = services.BuildServiceProvider();

            var writer = new OpenXmlWriterEngine(provider);
            var data = new[]
            {
                new DropdownItem { Status = "Active" }
            };

            var bytes = await writer.Write(data, "TestSheet");

            using var ms = new MemoryStream(bytes);
            using var doc = SpreadsheetDocument.Open(ms, false);

            var wbPart = doc.WorkbookPart;
            wbPart.Should().NotBeNull();

            var wsPart = wbPart!.WorksheetParts.First();
            var dataValidations = wsPart.Worksheet.Elements<DataValidations>().FirstOrDefault();
            dataValidations.Should().NotBeNull();
            dataValidations!.Count?.Value.Should().Be(1U);

            var dv = dataValidations.Elements<DataValidation>().First();
            dv.Type?.Value.Should().Be(DataValidationValues.List);
            dv.SequenceOfReferences?.InnerText.Should().Be("A2:A1000");
            dv.GetFirstChild<Formula1>()?.Text.Should().Be("\"Active,Inactive,Suspended\"");
        }

        [Fact]
        public async Task OpenXml_ReferenceSheetDropdown_GeneratesHiddenSheetAndFormula()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ExcelProfile<DropdownItem>>(new ReferenceDropdownProfile());
            var provider = services.BuildServiceProvider();

            var writer = new OpenXmlWriterEngine(provider);
            var data = new[]
            {
                new DropdownItem { Status = "Active", Branch = "Tehran, Center" }
            };

            var bytes = await writer.Write(data, "Orders");

            using var ms = new MemoryStream(bytes);
            using var doc = SpreadsheetDocument.Open(ms, false);

            var wbPart = doc.WorkbookPart;
            wbPart.Should().NotBeNull();

            // Verify workbook has 2 sheets: Orders (first) and _ValidationData (last, Hidden)
            var sheets = wbPart!.Workbook.Sheets!.Elements<Sheet>().ToList();
            sheets.Should().HaveCount(2);
            sheets[0].Name?.Value.Should().Be("Orders");
            sheets[1].Name?.Value.Should().Be("_ValidationData");

            var refSheet = sheets[1];
            refSheet.Should().NotBeNull();
            refSheet.State?.Value.Should().Be(SheetStateValues.Hidden);

            // Verify main sheet data validations
            var mainWsPart = (WorksheetPart)wbPart.GetPartById(sheets.First(s => s.Name == "Orders").Id!);
            var dataValidations = mainWsPart.Worksheet.Elements<DataValidations>().FirstOrDefault();
            dataValidations.Should().NotBeNull();
            dataValidations!.Count?.Value.Should().Be(2U);

            var validations = dataValidations.Elements<DataValidation>().ToList();

            // Column 1 is inline
            validations[0].SequenceOfReferences?.InnerText.Should().Be("A2:A1000");
            validations[0].GetFirstChild<Formula1>()?.Text.Should().Be("\"Active,Inactive\"");

            // Column 2 is reference sheet
            validations[1].SequenceOfReferences?.InnerText.Should().Be("B2:B1000");
            validations[1].GetFirstChild<Formula1>()?.Text.Should().Be("'_ValidationData'!$A$1:$A$3");
        }

        [Fact]
        public async Task EPPlus_InlineAndReferenceDropdowns_GenerateSuccessfully()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ExcelProfile<DropdownItem>>(new ReferenceDropdownProfile());
            var provider = services.BuildServiceProvider();

            var writer = new EPPlusWriterEngine(provider);
            var data = new[]
            {
                new DropdownItem { Status = "Active", Branch = "Tehran, Center" }
            };

            var bytes = await writer.Write(data, "Orders");

            using var ms = new MemoryStream(bytes);
            using var package = new ExcelPackage(ms);

            // Verify workbook sheets order: Orders first, _ValidationData last
            package.Workbook.Worksheets.Count.Should().Be(2);
            package.Workbook.Worksheets[0].Name.Should().Be("Orders");
            package.Workbook.Worksheets[1].Name.Should().Be("_ValidationData");

            // Verify hidden sheet exists
            var refWs = package.Workbook.Worksheets["_ValidationData"];
            refWs.Should().NotBeNull();
            refWs.Hidden.Should().Be(eWorkSheetHidden.Hidden);
            refWs.Cells[1, 1].Value.Should().Be("Tehran, Center");
            refWs.Cells[2, 1].Value.Should().Be("Isfahan, North");
            refWs.Cells[3, 1].Value.Should().Be("Shiraz, South");

            // Verify validations on main sheet
            var mainWs = package.Workbook.Worksheets["Orders"];
            mainWs.DataValidations.Count.Should().Be(2);

            var inlineVal = mainWs.DataValidations.FirstOrDefault(v => v.Address.Address == "A2:A1000");
            inlineVal.Should().NotBeNull();

            var refVal = mainWs.DataValidations.FirstOrDefault(v => v.Address.Address == "B2:B1000") as OfficeOpenXml.DataValidation.ExcelDataValidationList;
            refVal.Should().NotBeNull();
            refVal!.Formula.ExcelFormula.Should().Be("'_ValidationData'!$A$1:$A$3");
        }

        private class EmptyDropdownProfile : ExcelProfile<DropdownItem>
        {
            public EmptyDropdownProfile()
            {
                Map(x => x.Status).ToColumn(1);
            }
        }

        [Fact]
        public void OpenXmlDataValidationWriter_EdgeCases_Covered()
        {
            using var ms = new MemoryStream();
            using var doc = SpreadsheetDocument.Create(ms, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
            var wbPart = doc.AddWorkbookPart();
            var wsPart = wbPart.AddNewPart<WorksheetPart>();

            using (var writer = DocumentFormat.OpenXml.OpenXmlWriter.Create(wsPart))
            {
                writer.WriteStartElement(new DocumentFormat.OpenXml.Spreadsheet.Worksheet());

                // 1. Empty dropdown columns -> early return
                OpenXmlDataValidationWriter.WriteDataValidations(writer, new Dictionary<int, ColumnStyle>(), 10);

                // 2. Column with dropdown and refRanges == null
                var styles = new Dictionary<int, ColumnStyle>
                {
                    [1] = new ColumnStyle { Dropdown = new DropdownConfiguration(new[] { "A", "B" }) }
                };
                OpenXmlDataValidationWriter.WriteDataValidations(writer, styles, 10, null);

                writer.WriteEndElement();
            }
        }

        [Fact]
        public async Task DataValidationWriterHandler_EdgeCases_Covered()
        {
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Sheet1");
            // Pre-create _ValidationData to test the "??" branch
            package.Workbook.Worksheets.Add("_ValidationData");

            var profile = new ReferenceDropdownProfile();
            profile.EnsureBuilt();

            var context = new Exceler.Pipeline.Write.WriteContext<DropdownItem>
            {
                Worksheet = ws,
                Profile = profile
            };

            var handler = new Exceler.Pipeline.Write.Handlers.DataValidationWriterHandler<DropdownItem>();
            await handler.HandleAsync(context);

            ws.DataValidations.Count.Should().Be(2);

            // Also test when no dropdowns exist
            var emptyProfile = new EmptyDropdownProfile();
            emptyProfile.EnsureBuilt();
            var emptyContext = new Exceler.Pipeline.Write.WriteContext<DropdownItem>
            {
                Worksheet = ws,
                Profile = emptyProfile
            };
            await handler.HandleAsync(emptyContext);
        }
    }
}

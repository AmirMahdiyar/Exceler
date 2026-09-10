using Exceler.Configuration;
using Exceler.Tests.Common.Fixtures;
using Exceler.Tests.Common.Fixtures;
using Exceler.Tests.Common.TestDoubles.Models;
using Exceler.Tests.Common.TestDoubles.Models;
using FluentAssertions;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Exceler.Tests.Unit.Writers
{
    public class ExcelWriterEdgeCasesTests : ExcelerTestBase
    {
        [Fact]
        public async Task Empty_collection_exports_only_headers()
        {
            var emptyList = new List<TestModel>();

            byte[] excelBytes = await Writer.Write(emptyList);

            using var stream = new MemoryStream(excelBytes);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];

            worksheet.Dimension.Rows.Should().Be(1);
            worksheet.Cells[1, 1].Text.Should().Be("User ID");
            worksheet.Cells[1, 2].Text.Should().Be("Full Name");
        }

        [Fact]
        public async Task Properties_with_null_and_boolean_values_render_accurately()
        {
            var data = new List<EdgeCaseModel>
            {
                new EdgeCaseModel
                {
                    NullableInt = null,
                    IsActive = true,
                    Status = TestStatus.Pending
                }
            };

            byte[] excelBytes = await Writer.Write(data);

            using var stream = new MemoryStream(excelBytes);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];

            worksheet.Cells[2, 1].Value.Should().BeNull();
            worksheet.Cells[2, 2].Value.Should().Be(true);
        }

        [Fact]
        public async Task Exporting_empty_collection_generates_valid_spreadsheet_without_errors()
        {
            var sut = ExcelerTestBed.CreateWriter<EmptyModel, EmptyModelProfile>();
            var emptyData = new List<EmptyModel>();

            Func<Task<byte[]>> act = async () => await sut.Write(emptyData);

            var bytes = await act.Should().NotThrowAsync();
            bytes.Subject.Should().NotBeNullOrEmpty();
        }
    }

    public class EmptyModel
    {
    }

    public class EmptyModelProfile : ExcelProfile<EmptyModel>
    {
    }
}

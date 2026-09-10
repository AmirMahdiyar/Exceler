using Exceler.Configuration;
using Exceler.Tests.Common.Fixtures;
using FluentAssertions;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Exceler.Tests.Unit.Writers
{
    public class RtlCustomerModel
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
    }

    public class RtlCustomerProfile : ExcelProfile<RtlCustomerModel>
    {
        public RtlCustomerProfile()
        {
            WithRightToLeft();

            Map(x => x.CustomerId).ToColumn(1).WithHeader("Customer ID");
            Map(x => x.CustomerName).ToColumn(2).WithHeader("Customer Name");
        }
    }

    public class LtrDefaultCustomerModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class LtrDefaultCustomerProfile : ExcelProfile<LtrDefaultCustomerModel>
    {
        public LtrDefaultCustomerProfile()
        {
            Map(x => x.Id).ToColumn(1).WithHeader("ID");
            Map(x => x.Name).ToColumn(2).WithHeader("Name");
        }
    }

    public class CustomWidthProductModel
    {
        public int Code { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class CustomWidthProductProfile : ExcelProfile<CustomWidthProductModel>
    {
        public CustomWidthProductProfile()
        {
            WithAutoFitColumns(false);

            Map(x => x.Code).ToColumn(1).WithHeader("Code").WithWidth(15.0);
            Map(x => x.Description).ToColumn(2).WithHeader("Description").WithWidth(45.5);
        }
    }

    public class EmptySheetWidthModel
    {
        public int Serial { get; set; }
    }

    public class EmptySheetWidthProfile : ExcelProfile<EmptySheetWidthModel>
    {
        public EmptySheetWidthProfile()
        {
            WithAutoFitColumns(false);

            Map(x => x.Serial).ToColumn(1).WithHeader("Serial").WithWidth(32.0);
        }
    }

    public class ExcelWriterViewAndLayoutTests : ExcelerTestBase
    {
        [Fact]
        public async Task Worksheet_orientation_is_right_to_left_when_profile_specifies_rtl()
        {
            // Arrange
            var data = new List<RtlCustomerModel>
            {
                new() { CustomerId = 1, CustomerName = "Global Logistics Ltd" }
            };

            // Act
            var bytes = await Writer.Write(data);

            // Assert
            using var stream = new MemoryStream(bytes);
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            sheet.View.RightToLeft.Should().BeTrue();
        }

        [Fact]
        public async Task WhenProfileDoesNotSpecifyRightToLeft_WorksheetOrientationDefaultsToLeftToRight()
        {
            // Arrange
            var data = new List<LtrDefaultCustomerModel>
            {
                new() { Id = 1, Name = "Acme Corp" }
            };

            // Act
            var bytes = await Writer.Write(data);

            // Assert
            using var stream = new MemoryStream(bytes);
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            sheet.View.RightToLeft.Should().BeFalse();
        }

        [Fact]
        public async Task WhenProfileDisablesAutoFitColumns_ExplicitColumnWidthsArePreserved()
        {
            // Arrange
            var data = new List<CustomWidthProductModel>
            {
                new() { Code = 999, Description = "High-performance enterprise industrial router" }
            };

            // Act
            var bytes = await Writer.Write(data);

            // Assert
            using var stream = new MemoryStream(bytes);
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            sheet.Column(1).Width.Should().Be(15.0);
            sheet.Column(2).Width.Should().Be(45.5);
        }

        [Fact]
        public async Task WhenProfileConfiguresExplicitColumnWidth_WidthIsAppliedEvenOnEmptySheet()
        {
            // Arrange
            var emptyData = new List<EmptySheetWidthModel>();

            // Act
            var bytes = await Writer.Write(emptyData);

            // Assert
            using var stream = new MemoryStream(bytes);
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            sheet.Column(1).Width.Should().Be(32.0);
        }
    }
}

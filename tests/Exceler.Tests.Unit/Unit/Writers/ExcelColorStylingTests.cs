using Exceler.Configuration;
using FluentAssertions;
using System.Drawing;

namespace Exceler.Tests.Unit.Writers
{
    public class StyledColorModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class StyledColorModelProfile : ExcelProfile<StyledColorModel>
    {
        public StyledColorModelProfile()
        {
            Map(x => x.Id).ToColumn(1).WithHeader("ID").WithBackgroundColor("#FF0000");
            Map(x => x.Title).ToColumn(2).WithHeader("Title").WithFontColor("#0000FF");
        }
    }

    public class DrawingColorModelProfile : ExcelProfile<StyledColorModel>
    {
        public DrawingColorModelProfile()
        {
            Map(x => x.Id).ToColumn(1).WithBackgroundColor(Color.FromArgb(255, 0, 128, 0));
        }
    }

    public class EnumColorModelProfile : ExcelProfile<StyledColorModel>
    {
        public EnumColorModelProfile()
        {
            Map(x => x.Id).ToColumn(1).WithBackgroundColor(ExcelColor.SoftGreen);
            Map(x => x.Title).ToColumn(2).WithFontColor(ExcelColor.DarkBlue);
        }
    }

    public class StringColorNameModelProfile : ExcelProfile<StyledColorModel>
    {
        public StringColorNameModelProfile()
        {
            Map(x => x.Id).ToColumn(1).WithBackgroundColor("SoftYellow");
            Map(x => x.Title).ToColumn(2).WithFontColor("Navy");
        }
    }

    public class ExcelColorStylingTests
    {
        [Fact]
        public void WhenConfiguringColumnWithHexColors_HexValuesAreStoredInProfileStyles()
        {
            // Arrange & Act
            var profile = new StyledColorModelProfile();
            profile.EnsureBuilt();

            // Assert
            profile.ColumnStyles[1].BackgroundColorHex.Should().Be("#FF0000");
            profile.ColumnStyles[2].FontColorHex.Should().Be("#0000FF");
        }

        [Fact]
        public void WhenConfiguringColumnWithExcelColorEnum_CorrespondingHexValueIsStored()
        {
            // Arrange & Act
            var profile = new EnumColorModelProfile();
            profile.EnsureBuilt();

            // Assert
            profile.ColumnStyles[1].BackgroundColorHex.Should().Be("#E2EFDA");
            profile.ColumnStyles[2].FontColorHex.Should().Be("#1F4E78");
        }

        [Fact]
        public void WhenConfiguringColumnWithStringColorName_ColorNameIsStoredAndResolvedToHex()
        {
            // Arrange & Act
            var profile = new StringColorNameModelProfile();
            profile.EnsureBuilt();

            // Assert
            profile.ColumnStyles[1].BackgroundColorHex.Should().Be("SoftYellow");
            profile.ColumnStyles[2].FontColorHex.Should().Be("Navy");
            profile.ColumnStyles[1].BackgroundColor!.Value.R.Should().Be(255);
            profile.ColumnStyles[1].BackgroundColor!.Value.G.Should().Be(242);
            profile.ColumnStyles[1].BackgroundColor!.Value.B.Should().Be(204);
        }

        [Fact]
        public void WhenConfiguringColumnWithDrawingColor_EquivalentHexValueIsStored()
        {
            // Arrange & Act
            var profile = new DrawingColorModelProfile();
            profile.EnsureBuilt();

            // Assert
            profile.ColumnStyles[1].BackgroundColorHex.Should().Be("#008000");
        }

        [Fact]
        public void WhenReadingDrawingColorFromColumnStyle_CorrectColorInstanceIsReturned()
        {
            // Arrange
            var style = new ColumnStyle { BackgroundColorHex = "#FF0000" };

            // Act
            var color = style.BackgroundColor;

            // Assert
            color.Should().NotBeNull();
            color!.Value.R.Should().Be(255);
            color.Value.G.Should().Be(0);
            color.Value.B.Should().Be(0);
        }
    }
}

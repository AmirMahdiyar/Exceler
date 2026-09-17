using Exceler.Configuration;
using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace Exceler.Tests.Unit.Unit.Configuration
{
    public class DropdownConfigurationTests
    {
        private enum Priority
        {
            Low,
            Medium,
            High,
            Critical
        }

        private class DropdownTestModel
        {
            public string Status { get; set; } = string.Empty;
            public string PriorityName { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
        }

        private class DropdownTestProfile : ExcelProfile<DropdownTestModel>
        {
            public DropdownTestProfile()
            {
                Map(x => x.Status)
                    .ToColumn(1)
                    .WithDropdown("Pending", "Approved", "Rejected");

                Map(x => x.PriorityName)
                    .ToColumn(2)
                    .WithDropdownFromEnum<Priority>(cfg =>
                    {
                        cfg.ErrorTitle = "Invalid Priority";
                        cfg.ErrorMessage = "Select a valid priority level.";
                        cfg.ShowInputMessage = true;
                        cfg.InputTitle = "Priority";
                        cfg.InputMessage = "Choose priority";
                    });

                Map(x => x.City)
                    .ToColumn(3)
                    .WithDropdown(new[] { "Tehran, Capital", "Isfahan", "Shiraz" });
            }
        }

        [Fact]
        public void DropdownConfiguration_ValidOptions_InitializesCorrectly()
        {
            var options = new[] { "Option A", "  Option B  ", "Option A" }; // Has whitespace and duplicate
            var config = new DropdownConfiguration(options);

            config.Options.Should().HaveCount(2);
            config.Options.Should().ContainInOrder("Option A", "Option B");
            config.AllowBlank.Should().BeTrue();
            config.ShowErrorMessage.Should().BeTrue();
            config.StartRow.Should().Be(2);
            config.EndRow.Should().Be(1000);
            config.GetInlineFormula().Should().Be("\"Option A,Option B\"");
        }

        [Fact]
        public void DropdownConfiguration_ValidRowProperties_CanBeSet()
        {
            var config = new DropdownConfiguration(new[] { "A", "B" })
            {
                StartRow = 5,
                EndRow = 500,
                ShowInputMessage = true,
                InputTitle = "Input Title",
                InputMessage = "Input Message",
                ErrorTitle = "Error Title",
                ErrorMessage = "Error Message"
            };

            config.StartRow.Should().Be(5);
            config.EndRow.Should().Be(500);
            config.ShowInputMessage.Should().BeTrue();
            config.InputTitle.Should().Be("Input Title");
            config.InputMessage.Should().Be("Input Message");
            config.ErrorTitle.Should().Be("Error Title");
            config.ErrorMessage.Should().Be("Error Message");
        }

        [Fact]
        public void DropdownConfiguration_InvalidOptions_ThrowsAppropriateException()
        {
            Action actNull = () => new DropdownConfiguration(null!);
            actNull.Should().Throw<ArgumentNullException>();

            Action actEmpty = () => new DropdownConfiguration(Array.Empty<string>());
            actEmpty.Should().Throw<ArgumentException>();

            Action actWhitespace = () => new DropdownConfiguration(new[] { "  ", "\t", "" });
            actWhitespace.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void DropdownConfiguration_InvalidRows_ThrowsArgumentOutOfRangeException(int invalidRow)
        {
            var config = new DropdownConfiguration(new[] { "A", "B" });

            Action actStart = () => config.StartRow = invalidRow;
            actStart.Should().Throw<ArgumentOutOfRangeException>();

            Action actEnd = () => config.EndRow = invalidRow;
            actEnd.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void DropdownConfiguration_RequiresReferenceSheet_Logic()
        {
            // Small without comma -> inline
            var smallConfig = new DropdownConfiguration(new[] { "Yes", "No", "Maybe" });
            smallConfig.RequiresReferenceSheet.Should().BeFalse();

            // Option containing a comma -> requires reference sheet
            var commaConfig = new DropdownConfiguration(new[] { "Tehran, Center", "Isfahan" });
            commaConfig.RequiresReferenceSheet.Should().BeTrue();

            // Total string length > 255 chars -> requires reference sheet
            var longOptions = Enumerable.Range(1, 30).Select(i => $"LongDepartmentOptionNameNumber_{i:D2}").ToList();
            var longConfig = new DropdownConfiguration(longOptions);
            longConfig.RequiresReferenceSheet.Should().BeTrue();
        }

        [Fact]
        public void ColumnBuilder_DropdownRegistration_Succeeds()
        {
            var profile = new DropdownTestProfile();
            profile.EnsureBuilt();

            profile.ColumnStyles.Should().ContainKey(1);
            profile.ColumnStyles[1].Dropdown.Should().NotBeNull();
            profile.ColumnStyles[1].Dropdown!.Options.Should().ContainInOrder("Pending", "Approved", "Rejected");

            profile.ColumnStyles.Should().ContainKey(2);
            profile.ColumnStyles[2].Dropdown.Should().NotBeNull();
            profile.ColumnStyles[2].Dropdown!.Options.Should().ContainInOrder("Low", "Medium", "High", "Critical");
            profile.ColumnStyles[2].Dropdown!.ErrorTitle.Should().Be("Invalid Priority");

            profile.ColumnStyles.Should().ContainKey(3);
            profile.ColumnStyles[3].Dropdown.Should().NotBeNull();
            profile.ColumnStyles[3].Dropdown!.RequiresReferenceSheet.Should().BeTrue();
        }
    }
}

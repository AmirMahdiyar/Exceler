using Exceler.Configuration;
using FluentAssertions;
using System.Drawing;

namespace Exceler.Tests.Unit.Configuration
{
    public class ConfigurationEdgeCasesTests
    {
        private class SampleConfigModel
        {
            public string Name { get; set; } = string.Empty;
            public string ReadOnlyCode { get; } = "CODE-123";
        }

        private class SampleConfigProfile : ExcelProfile<SampleConfigModel>
        {
            public SampleConfigProfile()
            {
                Map(x => x.Name)
                    .ToColumn(1)
                    .WithHeader("Name")
                    .WithBackgroundColor(Color.CornflowerBlue)
                    .WithFontColor(Color.DarkGoldenrod);

                Map(x => x.ReadOnlyCode)
                    .ToColumn(2)
                    .WithHeader("Code");

                WithTrimStringValues(false);
                WithValidateTemplateOnRead(false);
            }
        }

        [Fact]
        public void Column_builder_sets_color_using_system_drawing_color_instances()
        {
            var sut = new SampleConfigProfile();
            sut.EnsureBuilt();

            sut.ColumnStyles[1].BackgroundColor!.Value.ToArgb().Should().Be(Color.CornflowerBlue.ToArgb());
            sut.ColumnStyles[1].FontColor!.Value.ToArgb().Should().Be(Color.DarkGoldenrod.ToArgb());
        }

        [Fact]
        public void Profile_honors_disabled_trimming_and_template_validation_flags()
        {
            var sut = new SampleConfigProfile();

            sut.TrimStringValues.Should().BeFalse();
            sut.ValidateTemplateOnRead.Should().BeFalse();
        }

        [Fact]
        public void Read_only_property_compiles_getter_without_throwing_when_setter_is_not_supported()
        {
            var sut = new SampleConfigProfile();
            sut.EnsureBuilt();

            sut.CompiledGetters.Should().ContainKey(2);
            var model = new SampleConfigModel();
            var value = sut.CompiledGetters[2](model);

            value.Should().Be("CODE-123");
        }

        [Fact]
        public void Column_style_allows_direct_assignment_and_retrieval_of_system_drawing_color()
        {
            var sut = new ColumnStyle
            {
                BackgroundColor = Color.Firebrick,
                FontColor = Color.GhostWhite
            };

            sut.BackgroundColor!.Value.ToArgb().Should().Be(Color.Firebrick.ToArgb());
            sut.FontColor!.Value.ToArgb().Should().Be(Color.GhostWhite.ToArgb());
            sut.BackgroundColorHex.Should().NotBeNullOrEmpty();
            sut.FontColorHex.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Column_style_returns_null_when_hex_is_null_or_empty()
        {
            var sut = new ColumnStyle();

            sut.BackgroundColor.Should().BeNull();
            sut.FontColor.Should().BeNull();
        }
    }
}

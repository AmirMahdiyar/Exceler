using Exceler.Core.Converter;
using FluentAssertions;
using System.Globalization;

namespace Exceler.Tests.Unit.Converter
{
    public class SafeConverterCultureTests
    {
        [Fact]
        public void WhenConvertingDecimalStringWithDotInGermanCulture_DotDecimalIsParsedCorrectly()
        {
            // Arrange
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("de-DE"); // German uses comma as decimal separator

                // Act
                var result = SafeConverter.ChangeType<decimal>("1234.56");

                // Assert
                result.Should().Be(1234.56m);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public void WhenConvertingDecimalStringWithCommaInGermanCulture_CommaDecimalIsParsedCorrectly()
        {
            // Arrange
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");

                // Act
                var result = SafeConverter.ChangeType<decimal>("1234,56");

                // Assert
                result.Should().Be(1234.56m);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public void WhenConvertingIsoDateTimeString_DateTimeIsParsedCorrectly()
        {
            // Act
            var result = SafeConverter.ChangeType<DateTime>("2026-08-31 14:30:00");

            // Assert
            result.Should().Be(new DateTime(2026, 8, 31, 14, 30, 0));
        }

        [Fact]
        public void WhenConvertingInvariantDateFormat_DateTimeIsParsedCorrectly()
        {
            // Act
            var result = SafeConverter.ChangeType<DateTime>("01/15/2026");

            // Assert
            result.Should().Be(new DateTime(2026, 1, 15));
        }

        [Fact]
        public void WhenConvertingDoubleStringWithDot_DoubleValueIsParsedCorrectly()
        {
            // Act
            var result = SafeConverter.ChangeType<double>("99.95");

            // Assert
            result.Should().Be(99.95);
        }

        [Theory]
        [InlineData("TRUE")]
        [InlineData("true")]
        [InlineData("1")]
        [InlineData("yes")]
        public void WhenConvertingBoolStringInTurkishCulture_BooleanIsTrue(string boolString)
        {
            // Arrange
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("tr-TR"); // Turkish culture has dotted/dotless I behavior

                // Act
                var result = SafeConverter.ChangeType<bool>(boolString);

                // Assert
                result.Should().BeTrue();
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }
    }
}

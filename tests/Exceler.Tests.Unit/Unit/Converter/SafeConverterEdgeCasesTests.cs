using Exceler.Core.Converter;
using Exceler.Core.Exceptions;
using FluentAssertions;
using System.Globalization;

namespace Exceler.Tests.Unit.Converter
{
    public class SafeConverterEdgeCasesTests
    {
        [Fact]
        public void TimeSpan_instance_is_preserved_when_converted_to_TimeSpan()
        {
            var expected = new TimeSpan(14, 30, 45);
            var sut = expected;

            var result = SafeConverter.ChangeType<TimeSpan>(sut);

            result.Should().Be(expected);
        }

        [Fact]
        public void TimeOnly_instance_is_converted_to_TimeSpan()
        {
            var timeOnly = new TimeOnly(15, 20, 10);
            var sut = timeOnly;

            var result = SafeConverter.ChangeType<TimeSpan>(sut);

            result.Should().Be(timeOnly.ToTimeSpan());
        }

        [Fact]
        public void DateTime_instance_provides_time_of_day_when_converted_to_TimeSpan()
        {
            var dateTime = new DateTime(2026, 9, 11, 8, 15, 30);
            var sut = dateTime;

            var result = SafeConverter.ChangeType<TimeSpan>(sut);

            result.Should().Be(dateTime.TimeOfDay);
        }

        [Fact]
        public void OADate_double_is_converted_to_TimeSpan()
        {
            var dateTime = new DateTime(1899, 12, 30, 6, 0, 0);
            var sut = dateTime.ToOADate();

            var result = SafeConverter.ChangeType<TimeSpan>(sut);

            result.Should().Be(new TimeSpan(6, 0, 0));
        }

        [Fact]
        public void Valid_time_string_is_parsed_to_TimeSpan()
        {
            var sut = "18:45:00";

            var result = SafeConverter.ChangeType<TimeSpan>(sut);

            result.Should().Be(new TimeSpan(18, 45, 0));
        }

        [Fact]
        public void Invalid_time_string_throws_ExcelCastException_for_TimeSpan()
        {
            var sut = "invalid-time-format";

            var act = () => SafeConverter.ChangeType<TimeSpan>(sut);

            act.Should().Throw<ExcelCastException>();
        }

        [Fact]
        public void DateTimeOffset_instance_is_preserved()
        {
            var expected = new DateTimeOffset(2026, 9, 11, 10, 0, 0, TimeSpan.FromHours(3.5));
            var sut = expected;

            var result = SafeConverter.ChangeType<DateTimeOffset>(sut);

            result.Should().Be(expected);
        }

        [Fact]
        public void DateTime_instance_is_converted_to_DateTimeOffset()
        {
            var dateTime = new DateTime(2026, 9, 11, 14, 0, 0);
            var sut = dateTime;

            var result = SafeConverter.ChangeType<DateTimeOffset>(sut);

            result.Should().Be(new DateTimeOffset(dateTime));
        }

        [Fact]
        public void OADate_double_is_converted_to_DateTimeOffset()
        {
            var dateTime = new DateTime(2026, 9, 11, 12, 0, 0);
            var sut = dateTime.ToOADate();

            var result = SafeConverter.ChangeType<DateTimeOffset>(sut);

            result.Should().Be(new DateTimeOffset(dateTime));
        }

        [Fact]
        public void Valid_iso_string_is_parsed_to_DateTimeOffset()
        {
            var sut = "2026-09-11T12:30:00+00:00";

            var result = SafeConverter.ChangeType<DateTimeOffset>(sut);

            result.Should().Be(DateTimeOffset.Parse(sut, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void Invalid_string_throws_ExcelCastException_for_DateTimeOffset()
        {
            var sut = "not-a-datetimeoffset";

            var act = () => SafeConverter.ChangeType<DateTimeOffset>(sut);

            act.Should().Throw<ExcelCastException>();
        }

        [Theory]
        [InlineData("0")]
        [InlineData("no")]
        [InlineData("false")]
        [InlineData("FALSE")]
        public void Falsy_strings_are_converted_to_false_boolean(string input)
        {
            var sut = input;

            var result = SafeConverter.ChangeType<bool>(sut);

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("1")]
        [InlineData("yes")]
        [InlineData("true")]
        [InlineData("TRUE")]
        public void Truthy_strings_are_converted_to_true_boolean(string input)
        {
            var sut = input;

            var result = SafeConverter.ChangeType<bool>(sut);

            result.Should().BeTrue();
        }

        [Fact]
        public void Unrecognized_bool_string_throws_ExcelCastException()
        {
            var sut = "maybe";

            var act = () => SafeConverter.ChangeType<bool>(sut);

            act.Should().Throw<ExcelCastException>();
        }

        [Theory]
        [InlineData("1234,56", 1234.56)]
        [InlineData("100,5", 100.5)]
        public void Comma_delimited_string_is_converted_to_decimal_under_comma_decimal_culture(string input, double expected)
        {
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");
                var sut = input;

                var result = SafeConverter.ChangeType<decimal>(sut);

                result.Should().Be((decimal)expected);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public void Integer_string_is_converted_to_decimal()
        {
            var sut = "4200";

            var result = SafeConverter.ChangeType<decimal>(sut);

            result.Should().Be(4200m);
        }

        [Fact]
        public void Float_number_is_converted_to_decimal()
        {
            float sut = 12.5f;

            var result = SafeConverter.ChangeType<decimal>(sut);

            result.Should().Be(12.5m);
        }

        [Fact]
        public void Long_number_is_converted_to_decimal()
        {
            long sut = 9999999999L;

            var result = SafeConverter.ChangeType<decimal>(sut);

            result.Should().Be(9999999999m);
        }

        [Theory]
        [InlineData("9876,54", 9876.54)]
        [InlineData("50,25", 50.25)]
        public void Comma_delimited_string_is_converted_to_double_under_comma_decimal_culture(string input, double expected)
        {
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");
                var sut = input;

                var result = SafeConverter.ChangeType<double>(sut);

                result.Should().BeApproximately(expected, 0.001);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public void Integer_string_is_converted_to_double()
        {
            var sut = "150";

            var result = SafeConverter.ChangeType<double>(sut);

            result.Should().Be(150.0);
        }

        [Fact]
        public void Float_number_is_converted_to_double()
        {
            float sut = 44.5f;

            var result = SafeConverter.ChangeType<double>(sut);

            result.Should().BeApproximately(44.5, 0.001);
        }

        [Fact]
        public void Decimal_number_is_converted_to_double()
        {
            decimal sut = 78.9m;

            var result = SafeConverter.ChangeType<double>(sut);

            result.Should().BeApproximately(78.9, 0.001);
        }

        [Fact]
        public void Long_number_is_converted_to_double()
        {
            long sut = 123456789L;

            var result = SafeConverter.ChangeType<double>(sut);

            result.Should().Be(123456789.0);
        }

        [Fact]
        public void Invalid_string_throws_ExcelCastException_for_Guid()
        {
            var sut = "not-a-valid-guid";

            var act = () => SafeConverter.ChangeType<Guid>(sut);

            act.Should().Throw<ExcelCastException>();
        }

        public enum SamplePriority
        {
            Low = 1,
            Medium = 2,
            High = 3
        }

        [Fact]
        public void Case_insensitive_string_is_parsed_to_enum()
        {
            var sut = "medium";

            var result = SafeConverter.ChangeType<SamplePriority>(sut);

            result.Should().Be(SamplePriority.Medium);
        }

        [Fact]
        public void Invalid_string_throws_ExcelCastException_for_enum()
        {
            var sut = "Critical";

            var act = () => SafeConverter.ChangeType<SamplePriority>(sut);

            act.Should().Throw<ExcelCastException>();
        }

        [Fact]
        public void Full_datetime_string_is_parsed_to_TimeOnly()
        {
            var sut = "2026-09-11 16:20:30";

            var result = SafeConverter.ChangeType<TimeOnly>(sut);

            result.Should().Be(new TimeOnly(16, 20, 30));
        }

        [Fact]
        public void TimeSpan_string_with_fractional_seconds_is_parsed_to_TimeOnly()
        {
            var sut = "10:15:30.123";

            var result = SafeConverter.ChangeType<TimeOnly>(sut);

            result.Hour.Should().Be(10);
            result.Minute.Should().Be(15);
            result.Second.Should().Be(30);
        }

        [Fact]
        public void Standard_time_string_is_parsed_to_TimeSpan()
        {
            var sut = "08:45:15";

            var result = SafeConverter.ChangeType<TimeSpan>(sut);

            result.Should().Be(new TimeSpan(8, 45, 15));
        }
    }
}

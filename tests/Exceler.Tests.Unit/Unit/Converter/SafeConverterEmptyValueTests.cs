using Exceler.Core.Converter;
using Exceler.Core.Exceptions;
using FluentAssertions;

namespace Exceler.Tests.Unit.Converter
{
    public class SafeConverterEmptyValueTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WhenConvertingEmptyValueToNonNullableInt_ExcelCastExceptionIsThrown(object? emptyValue)
        {
            // Act
            Action act = () => SafeConverter.ChangeType<int>(emptyValue);

            // Assert
            act.Should().Throw<ExcelCastException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WhenConvertingEmptyValueToNonNullableDecimal_ExcelCastExceptionIsThrown(object? emptyValue)
        {
            // Act
            Action act = () => SafeConverter.ChangeType<decimal>(emptyValue);

            // Assert
            act.Should().Throw<ExcelCastException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WhenConvertingEmptyValueToNonNullableDateTime_ExcelCastExceptionIsThrown(object? emptyValue)
        {
            // Act
            Action act = () => SafeConverter.ChangeType<DateTime>(emptyValue);

            // Assert
            act.Should().Throw<ExcelCastException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WhenConvertingEmptyValueToNonNullableBool_ExcelCastExceptionIsThrown(object? emptyValue)
        {
            // Act
            Action act = () => SafeConverter.ChangeType<bool>(emptyValue);

            // Assert
            act.Should().Throw<ExcelCastException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WhenConvertingEmptyValueToNonNullableGuid_ExcelCastExceptionIsThrown(object? emptyValue)
        {
            // Act
            Action act = () => SafeConverter.ChangeType<Guid>(emptyValue);

            // Assert
            act.Should().Throw<ExcelCastException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WhenConvertingEmptyValueToNullableInt_NullIsReturned(object? emptyValue)
        {
            // Act
            var result = SafeConverter.ChangeType<int?>(emptyValue);

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WhenConvertingEmptyValueToNullableDecimal_NullIsReturned(object? emptyValue)
        {
            // Act
            var result = SafeConverter.ChangeType<decimal?>(emptyValue);

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WhenConvertingEmptyValueToString_NullIsReturned(object? emptyValue)
        {
            // Act
            var result = SafeConverter.ChangeType<string>(emptyValue);

            // Assert
            result.Should().BeNull();
        }
    }
}

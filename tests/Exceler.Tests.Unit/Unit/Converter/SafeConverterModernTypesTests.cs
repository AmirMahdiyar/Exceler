using Exceler.Configuration;
using Exceler.Core.Converter;
using Exceler.Core.Exceptions;
using Exceler.Tests.Infrastructure.Base;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Exceler.Tests.Unit.Converter
{
    public class ModernScheduleModel
    {
        public int Id { get; set; }
        public DateOnly EventDate { get; set; }
        public DateOnly? OptionalDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly? OptionalTime { get; set; }
    }

    public class ModernScheduleProfile : ExcelProfile<ModernScheduleModel>
    {
        public ModernScheduleProfile()
        {
            Map(x => x.Id).ToColumn(1).WithHeader("ID");
            Map(x => x.EventDate).ToColumn(2).WithHeader("Event Date");
            Map(x => x.OptionalDate).ToColumn(3).WithHeader("Optional Date");
            Map(x => x.StartTime).ToColumn(4).WithHeader("Start Time");
            Map(x => x.OptionalTime).ToColumn(5).WithHeader("Optional Time");
        }
    }

    public class SafeConverterModernTypesTests : ExcelerTestBase
    {
        #region DateOnly Tests
        [Fact]
        public void WhenConvertingDateTimeToDateOnly_DateOnlyIsExtractedSuccessfully()
        {
            // Arrange
            var dateTime = new DateTime(2026, 9, 10, 15, 30, 0);

            // Act
            var result = SafeConverter.ChangeType<DateOnly>(dateTime);

            // Assert
            result.Should().Be(new DateOnly(2026, 9, 10));
        }

        [Fact]
        public void WhenConvertingDoubleOaDateToDateOnly_DateOnlyIsExtractedSuccessfully()
        {
            // Arrange
            double oaDate = new DateTime(2026, 5, 20).ToOADate();

            // Act
            var result = SafeConverter.ChangeType<DateOnly>(oaDate);

            // Assert
            result.Should().Be(new DateOnly(2026, 5, 20));
        }

        [Fact]
        public void WhenConvertingIsoDateStringToDateOnly_DateOnlyIsParsedSuccessfully()
        {
            // Act
            var result = SafeConverter.ChangeType<DateOnly>("2026-09-10");

            // Assert
            result.Should().Be(new DateOnly(2026, 9, 10));
        }

        [Fact]
        public void WhenConvertingFullDateTimeStringToDateOnly_DateOnlyIsParsedSuccessfully()
        {
            // Act
            var result = SafeConverter.ChangeType<DateOnly>("2026-09-10 18:45:00");

            // Assert
            result.Should().Be(new DateOnly(2026, 9, 10));
        }

        [Fact]
        public void WhenConvertingInvalidStringToDateOnly_ThrowsExcelCastException()
        {
            // Act
            Action act = () => SafeConverter.ChangeType<DateOnly>("invalid-date");

            // Assert
            act.Should().Throw<ExcelCastException>();
        }

        [Fact]
        public void WhenConvertingNullToNullableDateOnly_ReturnsNull()
        {
            // Act
            var result = SafeConverter.ChangeType<DateOnly?>(null);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void WhenConvertingEmptyStringToNonNullableDateOnly_ThrowsExcelCastException()
        {
            // Act
            Action act = () => SafeConverter.ChangeType<DateOnly>(string.Empty);

            // Assert
            act.Should().Throw<ExcelCastException>();
        }
        #endregion

        #region TimeOnly Tests
        [Fact]
        public void WhenConvertingTimeSpanToTimeOnly_TimeOnlyIsExtractedSuccessfully()
        {
            // Arrange
            var timeSpan = new TimeSpan(14, 30, 45);

            // Act
            var result = SafeConverter.ChangeType<TimeOnly>(timeSpan);

            // Assert
            result.Should().Be(new TimeOnly(14, 30, 45));
        }

        [Fact]
        public void WhenConvertingDateTimeToTimeOnly_TimeOnlyIsExtractedSuccessfully()
        {
            // Arrange
            var dateTime = new DateTime(2026, 1, 1, 9, 15, 0);

            // Act
            var result = SafeConverter.ChangeType<TimeOnly>(dateTime);

            // Assert
            result.Should().Be(new TimeOnly(9, 15, 0));
        }

        [Fact]
        public void WhenConvertingTimeStringToTimeOnly_TimeOnlyIsParsedSuccessfully()
        {
            // Act
            var result = SafeConverter.ChangeType<TimeOnly>("16:20:00");

            // Assert
            result.Should().Be(new TimeOnly(16, 20, 0));
        }

        [Fact]
        public void WhenConvertingInvalidStringToTimeOnly_ThrowsExcelCastException()
        {
            // Act
            Action act = () => SafeConverter.ChangeType<TimeOnly>("invalid-time");

            // Assert
            act.Should().Throw<ExcelCastException>();
        }

        [Fact]
        public void WhenConvertingNullToNullableTimeOnly_ReturnsNull()
        {
            // Act
            var result = SafeConverter.ChangeType<TimeOnly?>(null);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void WhenConvertingEmptyStringToNonNullableTimeOnly_ThrowsExcelCastException()
        {
            // Act
            Action act = () => SafeConverter.ChangeType<TimeOnly>("   ");

            // Assert
            act.Should().Throw<ExcelCastException>();
        }
        #endregion

        #region End-to-End Roundtrip Tests
        [Fact]
        public async Task WhenExportingAndImportingDateOnlyAndTimeOnlyModels_AllValuesRoundtripAccurately()
        {
            // Arrange
            var items = new List<ModernScheduleModel>
            {
                new()
                {
                    Id = 1,
                    EventDate = new DateOnly(2026, 9, 10),
                    OptionalDate = new DateOnly(2026, 12, 25),
                    StartTime = new TimeOnly(9, 30, 0),
                    OptionalTime = new TimeOnly(17, 0, 0)
                },
                new()
                {
                    Id = 2,
                    EventDate = new DateOnly(2026, 10, 1),
                    OptionalDate = null,
                    StartTime = new TimeOnly(13, 0, 0),
                    OptionalTime = null
                }
            };

            // Act: Write to Excel
            var excelBytes = await Writer.Write(items);

            // Act: Read back from Excel
            using var readStream = new MemoryStream(excelBytes);
            var results = Reader.Read<ModernScheduleModel, ModernScheduleModel>(readStream).ToList();

            // Assert
            results.Should().HaveCount(2);
            results.All(r => r.IsValid).Should().BeTrue();

            var first = results[0].Data!;
            first.Id.Should().Be(1);
            first.EventDate.Should().Be(new DateOnly(2026, 9, 10));
            first.OptionalDate.Should().Be(new DateOnly(2026, 12, 25));
            first.StartTime.Should().Be(new TimeOnly(9, 30, 0));
            first.OptionalTime.Should().Be(new TimeOnly(17, 0, 0));

            var second = results[1].Data!;
            second.Id.Should().Be(2);
            second.EventDate.Should().Be(new DateOnly(2026, 10, 1));
            second.OptionalDate.Should().BeNull();
            second.StartTime.Should().Be(new TimeOnly(13, 0, 0));
            second.OptionalTime.Should().BeNull();
        }
        #endregion
    }
}

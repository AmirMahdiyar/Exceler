using Exceler.Core.Exceptions;
using System.Globalization;

namespace Exceler.Core.Converter
{
    internal static class SafeConverter
    {
        public static T? ChangeType<T>(object? value)
        {
            Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            bool isNullable = !typeof(T).IsValueType || Nullable.GetUnderlyingType(typeof(T)) != null;

            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                if (!isNullable)
                    throw new ExcelCastException();

                return default;
            }

            try
            {
                if (targetType == typeof(string)) return (T)(object)value.ToString()!;

                if (targetType == typeof(DateTime))
                {
                    if (value is double doubleDate) return (T)(object)DateTime.FromOADate(doubleDate);
                    if (value is DateTime dateValue) return (T)(object)dateValue;
                    if (value is DateOnly dateOnlyValue) return (T)(object)dateOnlyValue.ToDateTime(TimeOnly.MinValue);

                    var dateStr = value.ToString()!;
                    if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate) ||
                        DateTime.TryParse(dateStr, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsedDate))
                    {
                        return (T)(object)parsedDate;
                    }

                    throw new ExcelCastException();
                }

                if (targetType == typeof(DateOnly))
                {
                    if (value is DateOnly dateOnly) return (T)(object)dateOnly;
                    if (value is DateTime dt) return (T)(object)DateOnly.FromDateTime(dt);
                    if (value is double doubleDate) return (T)(object)DateOnly.FromDateTime(DateTime.FromOADate(doubleDate));

                    var str = value.ToString()!.Trim();
                    if (DateOnly.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsedDateOnly) ||
                        DateOnly.TryParse(str, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsedDateOnly))
                    {
                        return (T)(object)parsedDateOnly;
                    }

                    if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDt) ||
                        DateTime.TryParse(str, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsedDt))
                    {
                        return (T)(object)DateOnly.FromDateTime(parsedDt);
                    }

                    throw new ExcelCastException();
                }

                if (targetType == typeof(TimeOnly))
                {
                    if (value is TimeOnly timeOnly) return (T)(object)timeOnly;
                    if (value is TimeSpan ts) return (T)(object)TimeOnly.FromTimeSpan(ts);
                    if (value is DateTime dt) return (T)(object)TimeOnly.FromDateTime(dt);
                    if (value is double doubleTime)
                    {
                        var parsedDt = DateTime.FromOADate(doubleTime);
                        return (T)(object)TimeOnly.FromDateTime(parsedDt);
                    }

                    var str = value.ToString()!.Trim();
                    if (TimeOnly.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly parsedTimeOnly) ||
                        TimeOnly.TryParse(str, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsedTimeOnly))
                    {
                        return (T)(object)parsedTimeOnly;
                    }

                    if (TimeSpan.TryParse(str, CultureInfo.InvariantCulture, out TimeSpan parsedTimeSpan) ||
                        TimeSpan.TryParse(str, CultureInfo.CurrentCulture, out parsedTimeSpan))
                    {
                        return (T)(object)TimeOnly.FromTimeSpan(parsedTimeSpan);
                    }

                    if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDateTime) ||
                        DateTime.TryParse(str, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsedDateTime))
                    {
                        return (T)(object)TimeOnly.FromDateTime(parsedDateTime);
                    }

                    throw new ExcelCastException();
                }

                if (targetType == typeof(TimeSpan))
                {
                    if (value is TimeSpan ts) return (T)(object)ts;
                    if (value is TimeOnly to) return (T)(object)to.ToTimeSpan();
                    if (value is DateTime dt) return (T)(object)dt.TimeOfDay;
                    if (value is double doubleTime) return (T)(object)DateTime.FromOADate(doubleTime).TimeOfDay;

                    var str = value.ToString()!.Trim();
                    if (TimeSpan.TryParse(str, CultureInfo.InvariantCulture, out TimeSpan parsedTs) ||
                        TimeSpan.TryParse(str, CultureInfo.CurrentCulture, out parsedTs))
                    {
                        return (T)(object)parsedTs;
                    }

                    if (TimeOnly.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly parsedTo) ||
                        TimeOnly.TryParse(str, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsedTo))
                    {
                        return (T)(object)parsedTo.ToTimeSpan();
                    }

                    throw new ExcelCastException();
                }

                if (targetType == typeof(DateTimeOffset))
                {
                    if (value is DateTimeOffset dto) return (T)(object)dto;
                    if (value is DateTime dt) return (T)(object)(new DateTimeOffset(dt));
                    if (value is double doubleDate) return (T)(object)(new DateTimeOffset(DateTime.FromOADate(doubleDate)));

                    var str = value.ToString()!.Trim();
                    if (DateTimeOffset.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset parsedDto) ||
                        DateTimeOffset.TryParse(str, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsedDto))
                    {
                        return (T)(object)parsedDto;
                    }

                    throw new ExcelCastException();
                }

                if (targetType == typeof(Guid))
                {
                    if (Guid.TryParse(value.ToString(), out Guid parsedGuid)) return (T)(object)parsedGuid;
                    throw new ExcelCastException();
                }

                if (targetType.IsEnum)
                {
                    return (T)Enum.Parse(targetType, value.ToString()!, true);
                }

                if (targetType == typeof(bool))
                {
                    var str = value.ToString()!.Trim();
                    if (string.Equals(str, "1", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(str, "yes", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(str, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        return (T)(object)true;
                    }

                    if (string.Equals(str, "0", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(str, "no", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(str, "false", StringComparison.OrdinalIgnoreCase))
                    {
                        return (T)(object)false;
                    }
                }

                if (targetType == typeof(decimal))
                {
                    if (value is decimal dec) return (T)(object)dec;
                    if (value is double d) return (T)(object)(decimal)d;
                    if (value is float f) return (T)(object)(decimal)f;
                    if (value is int i) return (T)(object)(decimal)i;
                    if (value is long l) return (T)(object)(decimal)l;

                    var str = value.ToString()!.Trim();
                    if (str.Contains('.'))
                    {
                        if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal dotDec))
                            return (T)(object)dotDec;
                    }
                    else if (str.Contains(','))
                    {
                        if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal commaDec))
                            return (T)(object)commaDec;

                        var normalized = str.Replace(',', '.');
                        if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal normalizedDec))
                            return (T)(object)normalizedDec;
                    }
                    else
                    {
                        if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal intDec))
                            return (T)(object)intDec;
                    }

                    throw new ExcelCastException();
                }

                if (targetType == typeof(double))
                {
                    if (value is double d) return (T)(object)d;
                    if (value is float f) return (T)(object)(double)f;
                    if (value is decimal dec) return (T)(object)(double)dec;
                    if (value is int i) return (T)(object)(double)i;
                    if (value is long l) return (T)(object)(double)l;

                    var str = value.ToString()!.Trim();
                    if (str.Contains('.'))
                    {
                        if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out double dotDbl))
                            return (T)(object)dotDbl;
                    }
                    else if (str.Contains(','))
                    {
                        if (double.TryParse(str, NumberStyles.Any, CultureInfo.CurrentCulture, out double commaDbl))
                            return (T)(object)commaDbl;

                        var normalized = str.Replace(',', '.');
                        if (double.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out double normalizedDbl))
                            return (T)(object)normalizedDbl;
                    }
                    else
                    {
                        if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out double intDbl))
                            return (T)(object)intDbl;
                    }

                    throw new ExcelCastException();
                }

                try
                {
                    return (T)Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                    return (T)Convert.ChangeType(value, targetType, CultureInfo.CurrentCulture);
                }
            }
            catch (Exception)
            {
                throw new ExcelCastException();
            }
        }
    }
}

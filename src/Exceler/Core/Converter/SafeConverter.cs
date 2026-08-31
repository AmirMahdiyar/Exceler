using Exceler.Core.Exceptions;

namespace Exceler.Core.Converter
{
    internal static class SafeConverter
    {
        public static T? ChangeType<T>(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return default;

            Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            try
            {
                if (targetType == typeof(string)) return (T)(object)value.ToString()!;

                if (targetType == typeof(DateTime))
                {
                    if (value is double doubleDate) return (T)(object)DateTime.FromOADate(doubleDate);
                    if (value is DateTime dateValue) return (T)(object)dateValue;
                    if (DateTime.TryParse(value.ToString(), out DateTime parsedDate)) return (T)(object)parsedDate;

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
                    var str = value.ToString()!.Trim().ToLower();
                    if (str == "1" || str == "yes" || str == "true") return (T)(object)true;
                    if (str == "0" || str == "no" || str == "false") return (T)(object)false;
                }

                return (T)Convert.ChangeType(value, targetType);
            }
            catch (Exception)
            {
                throw new ExcelCastException();
            }
        }
    }
}

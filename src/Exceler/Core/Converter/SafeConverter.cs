using Exceler.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Core.Converter
{
    internal static class SafeConverter
    {
        public static T? ChangeType<T>(object? value)
        {
            if (value == null || value == DBNull.Value)
                return default;

            if (value is T typedValue)
                return typedValue;

            Type underlyingType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            try
            {
                if (underlyingType == typeof(string))
                    return (T)(object)value.ToString()!;

                if (underlyingType.IsEnum)
                    return (T)Enum.Parse(underlyingType, value.ToString()!);

                return (T)Convert.ChangeType(value, underlyingType);
            }
            catch
            {
                throw new ExcelCastException();
            }
        }
    }
}

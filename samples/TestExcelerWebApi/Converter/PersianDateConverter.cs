using Exceler.Abstractions;
using System.Globalization;

namespace TestExcelerWebApi.Converter
{
    public class PersianDateConverter : IExcelValueConverter<string>
    {
        public string? ConvertFromExcel(object? value)
        {
            if (value is DateTime dt)
            {
                var pc = new PersianCalendar();
                return $"{pc.GetYear(dt)}/{pc.GetMonth(dt):00}/{pc.GetDayOfMonth(dt):00}";
            }

            if (value is string strDate && DateTime.TryParse(strDate, out var parsedDt))
            {
                var pc = new PersianCalendar();
                return $"{pc.GetYear(parsedDt)}/{pc.GetMonth(parsedDt):00}/{pc.GetDayOfMonth(parsedDt):00}";
            }

            return value?.ToString();
        }

        public object? ConvertToExcel(string? value)
        {
            return value;
        }
    }
}

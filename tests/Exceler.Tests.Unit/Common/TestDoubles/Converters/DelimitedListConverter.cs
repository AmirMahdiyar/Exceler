using Exceler.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace Exceler.Tests.Common.TestDoubles.Converters
{
    /// <summary>
    /// Test double converter used to verify custom value conversion in unit tests.
    /// </summary>
    public class DelimitedListConverter : IExcelValueConverter<List<string>>
    {
        private readonly string _delimiter;

        public DelimitedListConverter(string delimiter = ",") => _delimiter = delimiter;

        public List<string>? ConvertFromExcel(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return new List<string>();

            return value.ToString()!
                .Split(_delimiter, System.StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
        }

        public object? ConvertToExcel(List<string>? value)
        {
            if (value == null || !value.Any()) return null;
            return string.Join(_delimiter, value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Exceler.Abstractions;

namespace TestExcelerWebApi.Converter
{
    /// <summary>
    /// Custom value converter for complex collection properties.
    /// Converts delimited strings (comma, semicolon, or pipe) in Excel cells to/from List of strings.
    /// Demonstrates the IExcelValueConverter interface for advanced types.
    /// </summary>
    public class DelimitedTagsConverter : IExcelValueConverter<List<string>>
    {
        private static readonly char[] Delimiters = new[] { ',', ';', '|' };

        public List<string>? ConvertFromExcel(object? value)
        {
            if (value == null)
            {
                return new List<string>();
            }

            var text = value.ToString();
            if (string.IsNullOrWhiteSpace(text))
            {
                return new List<string>();
            }

            return text
                .Split(Delimiters, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public object? ConvertToExcel(List<string>? value)
        {
            if (value == null || value.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(", ", value);
        }
    }
}

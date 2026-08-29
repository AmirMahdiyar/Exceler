using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Abstractions
{
    /// <summary>
    /// Defines a custom converter to handle complex cell value transformations during import and export.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property in the C# model.</typeparam>
    public interface IExcelValueConverter<TProperty>
    {
        /// <summary>
        /// Converts the raw cell value from the Excel worksheet into the target property type.
        /// </summary>
        /// <param name="value">The raw value extracted from the Excel cell (e.g., string, double, DateTime).</param>
        /// <returns>The converted value to be assigned to the model property.</returns>
        TProperty? ConvertFromExcel(object? value);

        /// <summary>
        /// Converts the model property value into a format suitable for the Excel cell.
        /// </summary>
        /// <param name="value">The property value from the C# model.</param>
        /// <returns>The value to be written into the Excel cell.</returns>
        object? ConvertToExcel(TProperty? value);
    }
}

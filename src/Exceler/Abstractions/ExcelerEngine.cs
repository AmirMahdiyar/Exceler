namespace Exceler.Abstractions
{
    /// <summary>
    /// Specifies the underlying spreadsheet engine used for Excel workbook generation and export operations.
    /// </summary>
    public enum ExcelerEngine
    {
        /// <summary>
        /// High-performance, O(1) memory SAX-based streaming engine utilizing Microsoft's DocumentFormat.OpenXml.
        /// Ideal for high-volume exports (millions of rows) with minimal memory footprint.
        /// </summary>
        OpenXml = 0,

        /// <summary>
        /// Feature-rich, DOM-based engine utilizing EPPlus.
        /// Supports full workbook manipulation and runtime font-metric column auto-fitting (AutoFitColumns).
        /// </summary>
        EPPlus = 1
    }
}

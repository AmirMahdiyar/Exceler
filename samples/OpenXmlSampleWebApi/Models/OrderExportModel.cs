using System;

namespace OpenXmlSampleWebApi.Models
{
    /// <summary>
    /// Represents an e-commerce order record exported to Excel.
    /// </summary>
    public class OrderExportModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateOnly OrderDate { get; set; }
        public TimeOnly OrderTime { get; set; }
        public bool IsExpressDelivery { get; set; }
        public int ItemCount { get; set; }
        public string? Notes { get; set; }
    }
}

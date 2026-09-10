using System;
using System.Collections.Generic;

namespace TestExcelerWebApi.Inputs
{
    /// <summary>
    /// Represents a comprehensive, real-world Excel input model for financial and logistics order processing.
    /// Demonstrates support for Guid, DateOnly, TimeOnly, DateTime, Enums, Decimals, Bools, and complex collections.
    /// </summary>
    public class FinancialOrderInput
    {
        public Guid TrackingCode { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateOnly OrderDate { get; set; }
        public DateOnly? DeliveryDate { get; set; }
        public TimeOnly PreferredDeliveryTime { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentMethod Payment { get; set; }
        public PriorityLevel Priority { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal TaxRate { get; set; }
        public bool IsExpressDelivery { get; set; }
        public bool IsInsured { get; set; }
        public List<string> Tags { get; set; } = new();
        public string? Notes { get; set; }
    }
}

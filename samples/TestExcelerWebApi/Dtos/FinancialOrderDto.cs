using System;
using System.Collections.Generic;
using TestExcelerWebApi.Inputs;

namespace TestExcelerWebApi.Dtos
{
    /// <summary>
    /// Represents the enriched output DTO after asynchronous processing, validation, and financial calculations.
    /// Used for domain operations and styled Excel exports.
    /// </summary>
    public class FinancialOrderDto
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
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal InsuranceFee { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsExpressDelivery { get; set; }
        public bool IsInsured { get; set; }
        public List<string> Tags { get; set; } = new();
        public string SlaStatus { get; set; } = string.Empty;
        public DateTime ProcessedAtUtc { get; set; }
        public string? Notes { get; set; }
    }
}

using System;
using TestExcelerWebApi.Inputs;

namespace TestExcelerWebApi.Dtos
{
    /// <summary>
    /// An immutable C# 9+ record representing an order summary.
    /// Demonstrates both record features and parameterless constructor support
    /// so it can be both exported and imported seamlessly by Exceler.
    /// </summary>
    public record OrderSummaryRecord
    {
        public OrderSummaryRecord() { }

        public OrderSummaryRecord(
            Guid trackingCode = default,
            string customerCode = "",
            string customerName = "",
            DateOnly orderDate = default,
            TimeOnly preferredDeliveryTime = default,
            int quantity = 0,
            decimal totalAmount = 0,
            OrderStatus status = default,
            PriorityLevel priority = default,
            bool isExpressDelivery = false)
        {
            TrackingCode = trackingCode;
            CustomerCode = customerCode;
            CustomerName = customerName;
            OrderDate = orderDate;
            PreferredDeliveryTime = preferredDeliveryTime;
            Quantity = quantity;
            TotalAmount = totalAmount;
            Status = status;
            Priority = priority;
            IsExpressDelivery = isExpressDelivery;
        }

        public Guid TrackingCode { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateOnly OrderDate { get; set; }
        public TimeOnly PreferredDeliveryTime { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public PriorityLevel Priority { get; set; }
        public bool IsExpressDelivery { get; set; }
    }
}

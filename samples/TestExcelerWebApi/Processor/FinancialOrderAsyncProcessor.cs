using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Exceler.Abstractions;
using TestExcelerWebApi.Dtos;
using TestExcelerWebApi.Inputs;

namespace TestExcelerWebApi.Processor
{
    /// <summary>
    /// Demonstrates asynchronous data enrichment and transformation via IAsyncExcelProcessor.
    /// Performs financial calculations (subtotals, discounts, taxes, fees) and sets SLA metrics.
    /// </summary>
    public class FinancialOrderAsyncProcessor : IAsyncExcelProcessor<FinancialOrderInput, FinancialOrderDto>
    {
        public async Task<FinancialOrderDto> ProcessAsync(FinancialOrderInput input, CancellationToken cancellationToken = default)
        {
            // Simulate lightweight async enrichment (e.g. currency conversion, tax service)
            await Task.Delay(2, cancellationToken);

            var subtotal = Math.Round(input.Quantity * input.UnitPrice, 2);
            var discountAmount = Math.Round(subtotal * input.DiscountRate, 2);
            var taxableAmount = subtotal - discountAmount;
            var taxAmount = Math.Round(taxableAmount * input.TaxRate, 2);
            var shippingFee = input.IsExpressDelivery ? 25.00m : 5.00m;
            var insuranceFee = input.IsInsured ? 15.00m : 0.00m;
            var totalAmount = taxableAmount + taxAmount + shippingFee + insuranceFee;

            var slaStatus = input.Status switch
            {
                OrderStatus.Delivered => "Delivered",
                OrderStatus.Cancelled => "Void",
                _ when input.DeliveryDate.HasValue && input.DeliveryDate.Value <= DateOnly.FromDateTime(DateTime.UtcNow) => "Due Today / Overdue",
                _ => "On Track"
            };

            return new FinancialOrderDto
            {
                TrackingCode = input.TrackingCode,
                CustomerCode = input.CustomerCode,
                CustomerName = input.CustomerName,
                CompanyName = input.CompanyName,
                OrderDate = input.OrderDate,
                DeliveryDate = input.DeliveryDate,
                PreferredDeliveryTime = input.PreferredDeliveryTime,
                CreatedAtUtc = input.CreatedAtUtc,
                Status = input.Status,
                Payment = input.Payment,
                Priority = input.Priority,
                Quantity = input.Quantity,
                UnitPrice = input.UnitPrice,
                DiscountRate = input.DiscountRate,
                Subtotal = subtotal,
                DiscountAmount = discountAmount,
                TaxRate = input.TaxRate,
                TaxAmount = taxAmount,
                ShippingFee = shippingFee,
                InsuranceFee = insuranceFee,
                TotalAmount = totalAmount,
                IsExpressDelivery = input.IsExpressDelivery,
                IsInsured = input.IsInsured,
                Tags = new List<string>(input.Tags),
                SlaStatus = slaStatus,
                ProcessedAtUtc = DateTime.UtcNow,
                Notes = input.Notes
            };
        }
    }
}

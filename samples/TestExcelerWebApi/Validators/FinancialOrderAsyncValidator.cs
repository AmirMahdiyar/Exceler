using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Exceler.Abstractions;
using TestExcelerWebApi.Inputs;

namespace TestExcelerWebApi.Validators
{
    /// <summary>
    /// Demonstrates asynchronous domain validation via IAsyncExcelValidator.
    /// Can perform async lookups (e.g. database existence, external API validation, complex business rules).
    /// </summary>
    public class FinancialOrderAsyncValidator : IAsyncExcelValidator<FinancialOrderInput>
    {
        public async Task<IEnumerable<string>> ValidateAsync(FinancialOrderInput input, CancellationToken cancellationToken = default)
        {
            // Simulate lightweight async database/service validation call
            await Task.Delay(2, cancellationToken);

            var errors = new List<string>();

            if (input.TrackingCode == Guid.Empty)
            {
                errors.Add("TrackingCode must be a valid non-empty GUID.");
            }

            if (string.IsNullOrWhiteSpace(input.CustomerCode))
            {
                errors.Add("CustomerCode is mandatory.");
            }

            if (string.IsNullOrWhiteSpace(input.CustomerName))
            {
                errors.Add("CustomerName is mandatory.");
            }

            if (input.Quantity <= 0)
            {
                errors.Add($"Quantity must be greater than zero. Received: {input.Quantity}");
            }

            if (input.UnitPrice <= 0)
            {
                errors.Add($"UnitPrice must be greater than zero. Received: {input.UnitPrice:C}");
            }

            if (input.DiscountRate < 0 || input.DiscountRate > 1.0m)
            {
                errors.Add($"DiscountRate must be between 0.0 (0%) and 1.0 (100%). Received: {input.DiscountRate}");
            }

            if (input.TaxRate < 0 || input.TaxRate > 0.5m)
            {
                errors.Add($"TaxRate must be between 0.0 and 0.5 (50%). Received: {input.TaxRate}");
            }

            if (input.DeliveryDate.HasValue && input.DeliveryDate.Value < input.OrderDate)
            {
                errors.Add($"DeliveryDate ({input.DeliveryDate.Value}) cannot be earlier than OrderDate ({input.OrderDate}).");
            }

            return errors;
        }
    }
}

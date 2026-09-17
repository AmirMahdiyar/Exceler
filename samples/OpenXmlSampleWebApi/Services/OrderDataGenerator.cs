using Bogus;
using OpenXmlSampleWebApi.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace OpenXmlSampleWebApi.Services
{
    /// <summary>
    /// High-performance synthetic data generator powered by Bogus.
    /// Simulates enterprise database queries and high-volume data streams.
    /// </summary>
    public class OrderDataGenerator
    {
        private static readonly string[] Statuses = Enum.GetNames<OrderStatus>();
        private static readonly string[] Countries =
        {
            "United States",
            "Germany",
            "United Kingdom",
            "France",
            "Tehran, Iran",
            "Tokyo, Japan",
            "Sydney, Australia"
        };

        private static Faker<OrderExportModel> CreateFaker()
        {
            int currentId = 1;

            return new Faker<OrderExportModel>()
                .CustomInstantiator(f => new OrderExportModel())
                .RuleFor(o => o.Id, _ => currentId++)
                .RuleFor(o => o.OrderNumber, f => $"ORD-{f.Random.AlphaNumeric(8).ToUpper()}")
                .RuleFor(o => o.CustomerName, f => f.Name.FullName())
                .RuleFor(o => o.CustomerEmail, (f, o) => f.Internet.Email(o.CustomerName))
                .RuleFor(o => o.Country, f => f.PickRandom(Countries))
                .RuleFor(o => o.TotalAmount, f => Math.Round(f.Random.Decimal(19.99m, 4999.50m), 2))
                .RuleFor(o => o.Status, f => f.PickRandom(Statuses))
                .RuleFor(o => o.OrderDate, f => DateOnly.FromDateTime(f.Date.Recent(60)))
                .RuleFor(o => o.OrderTime, f => TimeOnly.FromTimeSpan(f.Date.Recent(1).TimeOfDay))
                .RuleFor(o => o.IsExpressDelivery, f => f.Random.Bool(0.25f))
                .RuleFor(o => o.ItemCount, f => f.Random.Int(1, 20))
                .RuleFor(o => o.Notes, f => f.Random.Bool(0.6f) ? f.Commerce.ProductDescription() : null);
        }

        /// <summary>
        /// Generates an in-memory collection of orders.
        /// </summary>
        /// <param name="count">Number of records to generate.</param>
        public IEnumerable<OrderExportModel> GenerateList(int count)
        {
            var faker = CreateFaker();
            return faker.Generate(count);
        }

        /// <summary>
        /// Asynchronously streams orders one-by-one without buffering the entire dataset in RAM.
        /// Simulates EF Core's AsNoTracking().AsAsyncEnumerable().
        /// </summary>
        /// <param name="count">Number of records to stream.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async IAsyncEnumerable<OrderExportModel> StreamOrdersAsync(
            int count,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var faker = CreateFaker();

            for (int i = 0; i < count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return faker.Generate();

                // Periodically yield execution to simulate async database I/O
                if ((i + 1) % 1000 == 0)
                {
                    await Task.Yield();
                }
            }
        }
    }
}

using Exceler.Abstractions;
using Exceler.Configuration;
using Exceler.Tests.Infrastructure;
using Exceler.Tests.Infrastructure.Base;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Exceler.Tests.Unit.Readers
{
    public class AsyncOrderInput
    {
        public int OrderId { get; set; }
        public string Customer { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }

    public class AsyncOrderDto
    {
        public int OrderId { get; set; }
        public string CustomerDisplay { get; set; } = string.Empty;
        public decimal TotalWithTax { get; set; }
    }

    public class AsyncOrderProfile : ExcelProfile<AsyncOrderInput>
    {
        public AsyncOrderProfile()
        {
            Map(x => x.OrderId).ToColumn(1).WithHeader("Order ID");
            Map(x => x.Customer).ToColumn(2).WithHeader("Customer Name");
            Map(x => x.TotalAmount).ToColumn(3).WithHeader("Amount");
        }
    }

    public class AsyncOrderProcessor : IAsyncExcelProcessor<AsyncOrderInput, AsyncOrderDto>
    {
        public async Task<AsyncOrderDto> ProcessAsync(AsyncOrderInput input, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);

            if (input.Customer == "THROW_ERROR")
                throw new InvalidOperationException("External service failed during processing.");

            return new AsyncOrderDto
            {
                OrderId = input.OrderId,
                CustomerDisplay = $"Customer: {input.Customer}",
                TotalWithTax = input.TotalAmount * 1.10m
            };
        }
    }

    public class AsyncExcelProcessorTests : ExcelerTestBase
    {
        [Fact]
        public void WhenAsyncProcessorIsDefinedInAssembly_ItIsAutomaticallyRegisteredInServiceCollection()
        {
            // Act
            var processor = ServiceProvider.GetService<IAsyncExcelProcessor<AsyncOrderInput, AsyncOrderDto>>();

            // Assert
            processor.Should().NotBeNull();
            processor.Should().BeOfType<AsyncOrderProcessor>();
        }

        [Fact]
        public async Task WhenAsyncProcessorTransformsModelInReadInChunksAsync_DataIsTransformedAsynchronously()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("AsyncProcessingSuccess");
            using var stream = builder
                .WithHeaders("Order ID", "Customer Name", "Amount")
                .WithRow(2, 101, "Alice", 100m)
                .WithRow(3, 102, "Bob", 200m)
                .Build();

            // Act
            var allChunks = new List<ExcelRowResult<AsyncOrderDto>>();
            await foreach (var chunk in Reader.ReadInChunksAsync<AsyncOrderInput, AsyncOrderDto>(stream, chunkSize: 10))
            {
                allChunks.AddRange(chunk);
            }

            // Assert
            allChunks.Should().HaveCount(2);

            allChunks[0].IsValid.Should().BeTrue();
            allChunks[0].Data!.CustomerDisplay.Should().Be("Customer: Alice");
            allChunks[0].Data!.TotalWithTax.Should().Be(110m);

            allChunks[1].IsValid.Should().BeTrue();
            allChunks[1].Data!.CustomerDisplay.Should().Be("Customer: Bob");
            allChunks[1].Data!.TotalWithTax.Should().Be(220m);
        }

        [Fact]
        public async Task WhenAsyncProcessorThrowsException_RowIsMarkedInvalidWithError()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("AsyncProcessingError");
            using var stream = builder
                .WithHeaders("Order ID", "Customer Name", "Amount")
                .WithRow(2, 201, "THROW_ERROR", 50m)
                .WithRow(3, 202, "Charlie", 70m)
                .Build();

            // Act
            var allChunks = new List<ExcelRowResult<AsyncOrderDto>>();
            await foreach (var chunk in Reader.ReadInChunksAsync<AsyncOrderInput, AsyncOrderDto>(stream, chunkSize: 10))
            {
                allChunks.AddRange(chunk);
            }

            // Assert
            allChunks.Should().HaveCount(2);

            allChunks[0].IsValid.Should().BeFalse();
            allChunks[0].Data.Should().BeNull();
            allChunks[0].Errors.Should().Contain(e => e.Contains("Processing error") && e.Contains("External service failed"));

            allChunks[1].IsValid.Should().BeTrue();
            allChunks[1].Data!.OrderId.Should().Be(202);
        }

        [Fact]
        public void WhenAsyncProcessorIsUsedInSynchronousRead_DataIsTransformedViaFallback()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("AsyncProcessingSyncFallback");
            using var stream = builder
                .WithHeaders("Order ID", "Customer Name", "Amount")
                .WithRow(2, 301, "Diana", 300m)
                .Build();

            // Act
            var results = Reader.Read<AsyncOrderInput, AsyncOrderDto>(stream).ToList();

            // Assert
            results.Should().ContainSingle();
            var result = results.First();

            result.IsValid.Should().BeTrue();
            result.Data!.CustomerDisplay.Should().Be("Customer: Diana");
            result.Data.TotalWithTax.Should().Be(330m);
        }

        [Fact]
        public async Task WhenCancellationTokenIsCancelledDuringAsyncProcessing_ThrowsOperationCanceledException()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("AsyncProcessingCancellation");
            using var stream = builder
                .WithHeaders("Order ID", "Customer Name", "Amount")
                .WithRow(2, 401, "Evan", 400m)
                .Build();

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            Func<Task> act = async () =>
            {
                await foreach (var _ in Reader.ReadInChunksAsync<AsyncOrderInput, AsyncOrderDto>(stream, chunkSize: 10, cancellationToken: cts.Token))
                {
                }
            };

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }
    }
}

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
    public class AsyncValidationModel
    {
        public int Id { get; set; }
        public int Score { get; set; }
    }

    public class AsyncValidationProfile : ExcelProfile<AsyncValidationModel>
    {
        public AsyncValidationProfile()
        {
            Map(x => x.Id).ToColumn(1).WithHeader("User ID");
            Map(x => x.Score).ToColumn(2).WithHeader("Test Score");
        }
    }

    public class AsyncValidationProcessor : IExcelProcessor<AsyncValidationModel, AsyncValidationModel>
    {
        public AsyncValidationModel Process(AsyncValidationModel input) => input;
    }

    public class AsyncModelValidator : IAsyncExcelValidator<AsyncValidationModel>
    {
        public async Task<IEnumerable<string>> ValidateAsync(AsyncValidationModel input, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);

            var errors = new List<string>();
            if (input.Score < 50)
            {
                errors.Add("Score must be at least 50 in async validation.");
            }

            return errors;
        }
    }

    public class AsyncExcelValidatorTests : ExcelerTestBase
    {
        [Fact]
        public void WhenAsyncValidatorIsDefinedInAssembly_ItIsAutomaticallyRegisteredInServiceCollection()
        {
            // Act
            var validator = ServiceProvider.GetService<IAsyncExcelValidator<AsyncValidationModel>>();

            // Assert
            validator.Should().NotBeNull();
            validator.Should().BeOfType<AsyncModelValidator>();
        }

        [Fact]
        public async Task WhenAsyncValidatorDetectsInvalidRowInReadInChunksAsync_RowIsMarkedInvalidWithValidationErrors()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("AsyncValidationErrors");
            using var stream = builder
                .WithHeaders("User ID", "Test Score")
                .WithRow(2, 1, 85)
                .WithRow(3, 2, 25)
                .WithRow(4, 3, 90)
                .Build();

            // Act
            var allChunks = new List<ExcelRowResult<AsyncValidationModel>>();
            await foreach (var chunk in Reader.ReadInChunksAsync<AsyncValidationModel, AsyncValidationModel>(stream, chunkSize: 10))
            {
                allChunks.AddRange(chunk);
            }

            // Assert
            allChunks.Should().HaveCount(3);

            allChunks[0].IsValid.Should().BeTrue();
            allChunks[0].Data!.Score.Should().Be(85);

            allChunks[1].IsValid.Should().BeFalse();
            allChunks[1].Data.Should().BeNull();
            allChunks[1].Errors.Should().Contain("Score must be at least 50 in async validation.");

            allChunks[2].IsValid.Should().BeTrue();
            allChunks[2].Data!.Score.Should().Be(90);
        }

        [Fact]
        public async Task WhenAsyncValidatorValidatesCleanDataInReadInChunksAsync_AllRowsPassValidation()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("AsyncValidationClean");
            using var stream = builder
                .WithHeaders("User ID", "Test Score")
                .WithRow(2, 1, 60)
                .WithRow(3, 2, 75)
                .Build();

            // Act
            var allChunks = new List<ExcelRowResult<AsyncValidationModel>>();
            await foreach (var chunk in Reader.ReadInChunksAsync<AsyncValidationModel, AsyncValidationModel>(stream, chunkSize: 10))
            {
                allChunks.AddRange(chunk);
            }

            // Assert
            allChunks.Should().HaveCount(2);
            allChunks.Should().OnlyContain(r => r.IsValid);
        }

        [Fact]
        public void WhenAsyncValidatorIsRegisteredForSynchronousRead_ValidationErrorsAreReportedViaFallback()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("AsyncValidationSyncFallback");
            using var stream = builder
                .WithHeaders("User ID", "Test Score")
                .WithRow(2, 1, 30)
                .Build();

            // Act
            var results = Reader.Read<AsyncValidationModel, AsyncValidationModel>(stream).ToList();

            // Assert
            results.Should().ContainSingle();
            var result = results.First();

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Score must be at least 50 in async validation.");
        }

        [Fact]
        public async Task WhenCancellationTokenIsCancelledDuringAsyncValidation_OperationThrowsOperationCanceledException()
        {
            // Arrange
            using var builder = new ExcelStreamBuilder("AsyncValidationCancellation");
            using var stream = builder
                .WithHeaders("User ID", "Test Score")
                .WithRow(2, 1, 80)
                .Build();

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            Func<Task> act = async () =>
            {
                await foreach (var _ in Reader.ReadInChunksAsync<AsyncValidationModel, AsyncValidationModel>(stream, chunkSize: 10, cancellationToken: cts.Token))
                {
                }
            };

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }
    }
}

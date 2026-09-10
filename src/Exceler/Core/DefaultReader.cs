using Exceler.Abstractions;
using Exceler.Configuration;
using Exceler.Core.Exceptions;
using Exceler.Pipeline.Read;
using Exceler.Pipeline.Read.Handlers;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Exceler.Core
{
    /// <summary>
    /// Default implementation of <see cref="IExcelReader"/> using EPPlus and a pipeline of read handlers.
    /// </summary>
    internal class DefaultReader : IExcelReader
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultReader"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve profiles, validators, and processors.</param>
        public DefaultReader(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public IEnumerable<ExcelRowResult<TOutput>> Read<TInput, TOutput>(Stream excelStream,
            string? sheetName = null) where TInput : class, new()
        {
            var (profile, processor, asyncProcessor, validator, asyncValidator) = ResolveDependencies<TInput, TOutput>();
            profile.EnsureBuilt();

            using var package = new ExcelPackage(excelStream);
            var worksheet = GetWorksheet(package, sheetName);
            if (worksheet.Dimension == null) yield break;

            ValidateHeaders(worksheet, profile);

            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;

            var activeSetters = profile.CompiledSetters
                .Where(s => s.Key <= colCount)
                .ToArray();

            if (activeSetters.Length == 0) yield break;

            var chain = BuildProcessingChain<TInput, TOutput>();

            for (int row = 2; row <= rowCount; row++)
            {
                if (!TryExtractRowValues(worksheet, row, activeSetters, out var rowValues))
                    continue;

                var context = new ReadContext<TInput, TOutput>(row)
                {
                    Worksheet = worksheet,
                    ColCount = colCount,
                    Profile = profile,
                    Processor = processor,
                    AsyncProcessor = asyncProcessor,
                    Validator = validator,
                    AsyncValidator = asyncValidator,
                    ActiveSetters = activeSetters,
                    RowValues = rowValues
                };

                chain.Handle(context);

                yield return context.Result;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<List<ExcelRowResult<TOutput>>> ReadInChunksAsync<TInput, TOutput>(
            Stream excelStream,
            int chunkSize = 10000,
            string? sheetName = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
            where TInput : class, new()
        {
            var (profile, processor, asyncProcessor, validator, asyncValidator) = ResolveDependencies<TInput, TOutput>();
            profile.EnsureBuilt();

            using var package = new ExcelPackage();
            await package.LoadAsync(excelStream, cancellationToken);

            var worksheet = GetWorksheet(package, sheetName);
            if (worksheet.Dimension == null) yield break;

            ValidateHeaders(worksheet, profile);

            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;

            var activeSetters = profile.CompiledSetters
                .Where(s => s.Key <= colCount)
                .ToArray();

            if (activeSetters.Length == 0) yield break;

            var currentChunk = new List<ExcelRowResult<TOutput>>(chunkSize);

            var chain = BuildProcessingChain<TInput, TOutput>();

            for (int row = 2; row <= rowCount; row++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!TryExtractRowValues(worksheet, row, activeSetters, out var rowValues))
                    continue;

                var context = new ReadContext<TInput, TOutput>(row)
                {
                    Worksheet = worksheet,
                    ColCount = colCount,
                    Profile = profile,
                    Processor = processor,
                    AsyncProcessor = asyncProcessor,
                    Validator = validator,
                    AsyncValidator = asyncValidator,
                    ActiveSetters = activeSetters,
                    RowValues = rowValues
                };

                await chain.HandleAsync(context, cancellationToken);

                currentChunk.Add(context.Result);

                if (currentChunk.Count == chunkSize)
                {
                    yield return currentChunk;
                    currentChunk = new List<ExcelRowResult<TOutput>>(chunkSize);
                    await Task.Yield();
                }
            }

            if (currentChunk.Any())
                yield return currentChunk;
        }

        #region Private Methods

        /// <summary>
        /// Resolves the mapping profile, validators, and processors for the given input and output types.
        /// </summary>
        /// <typeparam name="TInput">The input model type.</typeparam>
        /// <typeparam name="TOutput">The output model type.</typeparam>
        /// <returns>A tuple containing resolved profile, processors, and validators.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no processor is found and input cannot be cast to output.</exception>
        private (ExcelProfile<TInput>, IExcelProcessor<TInput, TOutput>?, IAsyncExcelProcessor<TInput, TOutput>?, IExcelValidator<TInput>?, IAsyncExcelValidator<TInput>?) ResolveDependencies<TInput, TOutput>()
            where TInput : class, new()
        {
            var profile = _serviceProvider.GetRequiredService<ExcelProfile<TInput>>();
            var processor = _serviceProvider.GetService<IExcelProcessor<TInput, TOutput>>();
            var asyncProcessor = _serviceProvider.GetService<IAsyncExcelProcessor<TInput, TOutput>>();
            var validator = _serviceProvider.GetService<IExcelValidator<TInput>>();
            var asyncValidator = _serviceProvider.GetService<IAsyncExcelValidator<TInput>>();

            if (processor == null && asyncProcessor == null)
            {
                if (typeof(TOutput).IsAssignableFrom(typeof(TInput)))
                {
                    var passThrough = new PassThroughProcessor<TInput, TOutput>();
                    processor = passThrough;
                    asyncProcessor = passThrough;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"No processor registered for converting '{typeof(TInput).FullName}' to '{typeof(TOutput).FullName}'. " +
                        $"When input and output types differ, an IExcelProcessor<{typeof(TInput).Name}, {typeof(TOutput).Name}> or IAsyncExcelProcessor<{typeof(TInput).Name}, {typeof(TOutput).Name}> must be implemented and registered in the dependency injection container.");
                }
            }

            return (profile, processor, asyncProcessor, validator, asyncValidator);
        }

        /// <summary>
        /// Extracts cell values for a given row across all active column setters.
        /// </summary>
        /// <typeparam name="TInput">The input model type.</typeparam>
        /// <param name="worksheet">The source worksheet.</param>
        /// <param name="row">The 1-based row index.</param>
        /// <param name="activeSetters">The array of active column setters.</param>
        /// <param name="rowValues">The extracted cell values for the row.</param>
        /// <returns>True if the row contains at least one non-empty value; otherwise, false.</returns>
        private static bool TryExtractRowValues<TInput>(
            ExcelWorksheet worksheet,
            int row,
            KeyValuePair<int, Action<TInput, object>>[] activeSetters,
            out object?[] rowValues) where TInput : class, new()
        {
            rowValues = new object?[activeSetters.Length];
            bool hasAnyValue = false;

            for (int i = 0; i < activeSetters.Length; i++)
            {
                var val = worksheet.Cells[row, activeSetters[i].Key].Value;
                if (val != null)
                {
                    if (val is string str)
                    {
                        if (!string.IsNullOrWhiteSpace(str))
                            hasAnyValue = true;
                    }
                    else
                    {
                        hasAnyValue = true;
                    }
                }
                rowValues[i] = val;
            }

            return hasAnyValue;
        }

        /// <summary>
        /// Retrieves the target worksheet from the package by name, or returns the first worksheet if name is omitted.
        /// </summary>
        /// <param name="package">The Excel package instance.</param>
        /// <param name="sheetName">The optional sheet name.</param>
        /// <returns>The resolved <see cref="ExcelWorksheet"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when a specific sheet name is provided but not found.</exception>
        private ExcelWorksheet GetWorksheet(ExcelPackage package, string? sheetName)
        {
            if (string.IsNullOrWhiteSpace(sheetName))
                return package.Workbook.Worksheets[0];

            var worksheet = package.Workbook.Worksheets[sheetName];
            if (worksheet == null)
                throw new ArgumentException($"Sheet with this ({sheetName}) not found !");

            return worksheet;
        }

        /// <summary>
        /// Validates worksheet headers against the configured profile column headers.
        /// </summary>
        /// <typeparam name="TInput">The input model type.</typeparam>
        /// <param name="worksheet">The worksheet to validate.</param>
        /// <param name="profile">The mapping profile containing expected headers.</param>
        /// <exception cref="ExcelTemplateMismatchException">Thrown when headers do not match the profile definition.</exception>
        private void ValidateHeaders<TInput>(ExcelWorksheet worksheet, ExcelProfile<TInput> profile) where TInput : class, new()
        {
            if (!profile.ValidateTemplateOnRead || profile.ColumnHeaders.Count == 0)
                return;

            var errors = new List<string>();

            foreach (var header in profile.ColumnHeaders)
            {
                int colIndex = header.Key;
                string expectedHeaderName = header.Value;

                string? actualHeaderName = worksheet.Cells[1, colIndex].Text?.Trim();

                if (!string.Equals(expectedHeaderName, actualHeaderName, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add("Input Header is not equal with excepted Header");
                }
            }

            if (errors.Any())
            {
                throw new ExcelTemplateMismatchException(errors);
            }
        }

        /// <summary>
        /// Assembles the Chain of Responsibility handlers for reading and processing Excel rows.
        /// </summary>
        /// <typeparam name="TInput">The input model type.</typeparam>
        /// <typeparam name="TOutput">The output model type.</typeparam>
        /// <returns>The head of the read handler pipeline.</returns>
        private ReadHandler<TInput, TOutput> BuildProcessingChain<TInput, TOutput>() where TInput : class, new()
        {
            var head = new ParseHandler<TInput, TOutput>();

            head.SetNext(new ValidateHandler<TInput, TOutput>())
                .SetNext(new ProcessHandler<TInput, TOutput>());

            return head;
        }

        #endregion
    }
}

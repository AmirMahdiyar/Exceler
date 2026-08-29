using Exceler.Abstractions;
using Exceler.Configuration;
using Exceler.Core.Exceptions;
using Exceler.Pipeline.Read;
using Exceler.Pipeline.Read.Handlers;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Core
{
    internal class DefaultReader : IExcelReader
    {
        private readonly IServiceProvider _serviceProvider;

        public DefaultReader(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public IEnumerable<ExcelRowResult<TOutput>> Read<TInput, TOutput>(Stream excelStream,
            string? sheetName = null) where TInput : class, new()
        {
            var (profile, processor, validator) = ResolveDependencies<TInput, TOutput>();
            profile.EnsureBuilt();

            using var package = new ExcelPackage(excelStream);
            var worksheet = GetWorksheet(package, sheetName);
            if (worksheet.Dimension == null) yield break;

            ValidateHeaders(worksheet, profile);

            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;
            var mappedColumns = profile.CompiledSetters.Keys;

            var chain = BuildProcessingChain<TInput, TOutput>();

            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(worksheet, row, mappedColumns, colCount))
                    continue;

                var context = new ReadContext<TInput, TOutput>(row)
                {
                    Worksheet = worksheet,
                    ColCount = colCount,
                    Profile = profile,
                    Processor = processor,
                    Validator = validator
                };

                chain.Handle(context);

                yield return context.Result;
            }
        }

        public async IAsyncEnumerable<List<ExcelRowResult<TOutput>>> ReadInChunksAsync<TInput, TOutput>(
            Stream excelStream,
            int chunkSize = 10000,
            string? sheetName = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
            where TInput : class, new()
        {
            var (profile, processor, validator) = ResolveDependencies<TInput, TOutput>();
            profile.EnsureBuilt();

            using var package = new ExcelPackage();
            await package.LoadAsync(excelStream, cancellationToken);

            var worksheet = GetWorksheet(package, sheetName);
            if (worksheet.Dimension == null) yield break;

            ValidateHeaders(worksheet, profile);

            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;
            var mappedColumns = profile.CompiledSetters.Keys;

            var currentChunk = new List<ExcelRowResult<TOutput>>(chunkSize);

            var chain = BuildProcessingChain<TInput, TOutput>();

            for (int row = 2; row <= rowCount; row++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (IsRowEmpty(worksheet, row, mappedColumns, colCount))
                    continue;

                var context = new ReadContext<TInput, TOutput>(row)
                {
                    Worksheet = worksheet,
                    ColCount = colCount,
                    Profile = profile,
                    Processor = processor,
                    Validator = validator
                };

                chain.Handle(context);

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
        private (ExcelProfile<TInput>, IExcelProcessor<TInput, TOutput>, IExcelValidator<TInput>?) ResolveDependencies<TInput, TOutput>()
            where TInput : class, new()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var profile = _serviceProvider.GetRequiredService<ExcelProfile<TInput>>();
            var processor = _serviceProvider.GetRequiredService<IExcelProcessor<TInput, TOutput>>();
            var validator = _serviceProvider.GetService<IExcelValidator<TInput>>();
            return (profile, processor, validator);
        }
        private bool IsRowEmpty(ExcelWorksheet worksheet, int row, IEnumerable<int> targetColumns, int maxColumn)
        {
            foreach (var col in targetColumns)
            {
                if (col <= maxColumn)
                {
                    var cellValue = worksheet.Cells[row, col].Value;

                    if (cellValue != null && !string.IsNullOrWhiteSpace(cellValue.ToString()))
                        return false;
                }
            }

            return true;
        }
        private ExcelWorksheet GetWorksheet(ExcelPackage package, string? sheetName)
        {
            if (string.IsNullOrWhiteSpace(sheetName))
                return package.Workbook.Worksheets[0];

            var worksheet = package.Workbook.Worksheets[sheetName];
            if (worksheet == null)
                throw new ArgumentException($"Sheet with this ({sheetName}) not found !");

            return worksheet;
        }
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

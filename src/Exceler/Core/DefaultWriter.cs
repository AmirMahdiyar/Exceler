using Exceler.Abstractions;
using Exceler.Configuration;
using Exceler.Pipeline.Write;
using Exceler.Pipeline.Write.Handlers;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Exceler.Core
{
    /// <summary>
    /// Default implementation of <see cref="IExcelWriter"/> utilizing EPPlus and a pipeline of write handlers.
    /// </summary>
    internal class DefaultWriter : IExcelWriter
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultWriter"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve mapping profiles.</param>
        public DefaultWriter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public async Task<byte[]> Write<TModel>(IEnumerable<TModel> data, string? sheetName = null) where TModel : class
        {
            var profile = GetProfile<TModel>();

            using var package = new ExcelPackage();
            await PopulatePackageAsync(package, profile, sheetName, data: data);

            return await package.GetAsByteArrayAsync();
        }

        /// <inheritdoc />
        public async Task WriteAsync<TModel>(IEnumerable<TModel> data, Stream outputStream, string? sheetName = null) where TModel : class
        {
            var profile = GetProfile<TModel>();

            using var package = new ExcelPackage();
            await PopulatePackageAsync(package, profile, sheetName, data: data);

            await package.SaveAsAsync(outputStream);
        }

        /// <inheritdoc />
        public async Task WriteAsync<TModel>(IAsyncEnumerable<TModel> dataStream, Stream outputStream, string? sheetName = null) where TModel : class
        {
            var profile = GetProfile<TModel>();
            using var package = new ExcelPackage();

            await PopulatePackageAsync(package, profile, sheetName, asyncData: dataStream);

            await package.SaveAsAsync(outputStream);
        }

        #region Private Methods

        /// <summary>
        /// Populates an Excel package with data, headers, and styles using the write pipeline chain.
        /// </summary>
        /// <typeparam name="TModel">The type of the model being exported.</typeparam>
        /// <param name="package">The Excel package instance.</param>
        /// <param name="profile">The model mapping and styling profile.</param>
        /// <param name="sheetName">The optional sheet name.</param>
        /// <param name="data">The optional synchronous data collection.</param>
        /// <param name="asyncData">The optional asynchronous data stream.</param>
        /// <returns>A task representing the asynchronous populate operation.</returns>
        private async Task PopulatePackageAsync<TModel>(
            ExcelPackage package,
            ExcelProfile<TModel> profile,
            string? sheetName,
            IEnumerable<TModel>? data = null,
            IAsyncEnumerable<TModel>? asyncData = null) where TModel : class
        {
            var finalSheetName = string.IsNullOrWhiteSpace(sheetName) ? "Sheet1" : sheetName;
            var worksheet = package.Workbook.Worksheets.Add(finalSheetName);

            var context = new WriteContext<TModel>
            {
                Worksheet = worksheet,
                Profile = profile,
                Data = data,
                AsyncData = asyncData
            };

            var chain = BuildWriteChain<TModel>();
            await chain.HandleAsync(context);
        }

        /// <summary>
        /// Assembles the Chain of Responsibility handlers for writing Excel files.
        /// </summary>
        /// <typeparam name="TModel">The type of the model being exported.</typeparam>
        /// <returns>The head of the write handler pipeline.</returns>
        private WriteHandler<TModel> BuildWriteChain<TModel>() where TModel : class
        {
            var head = new HeaderWriterHandler<TModel>();

            head.SetNext(new DataWriterHandler<TModel>())
                .SetNext(new StyleWriterHandler<TModel>())
                .SetNext(new FormattingWriterHandler<TModel>());

            return head;
        }

        /// <summary>
        /// Resolves and ensures expression compilation for the requested model's mapping profile.
        /// </summary>
        /// <typeparam name="TModel">The type of the model being exported.</typeparam>
        /// <returns>The compiled <see cref="ExcelProfile{TModel}"/> instance.</returns>
        private ExcelProfile<TModel> GetProfile<TModel>() where TModel : class
        {
            var profile = _serviceProvider.GetRequiredService<ExcelProfile<TModel>>();
            profile.EnsureBuilt();
            return profile;
        }

        #endregion
    }
}

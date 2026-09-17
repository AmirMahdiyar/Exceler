using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Exceler.Abstractions;
using Exceler.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Exceler.Core.OpenXml
{
    /// <summary>
    /// High-performance, $O(1)$ memory SAX-based facade of <see cref="IExcelWriter"/>.
    /// Orchestrates package building, stylesheet compilation, and streaming worksheet serialization
    /// without in-memory DOM allocations, enabling export of millions of rows with minimal memory consumption.
    /// </summary>
    internal class OpenXmlWriterEngine : IExcelWriter
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenXmlWriterEngine"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve model-specific <see cref="ExcelProfile{TModel}"/> instances.</param>
        public OpenXmlWriterEngine(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc />
        public async Task<byte[]> Write<TModel>(IEnumerable<TModel> data, string? sheetName = null) where TModel : class
        {
            using var memoryStream = new MemoryStream();
            await WriteInternalAsync(data, asyncDataStream: null, memoryStream, sheetName);
            return memoryStream.ToArray();
        }

        /// <inheritdoc />
        public async Task WriteAsync<TModel>(IEnumerable<TModel> data, Stream outputStream, string? sheetName = null) where TModel : class
        {
            await WriteInternalAsync(data, asyncDataStream: null, outputStream, sheetName);
        }

        /// <inheritdoc />
        public async Task WriteAsync<TModel>(IAsyncEnumerable<TModel> dataStream, Stream outputStream, string? sheetName = null) where TModel : class
        {
            await WriteInternalAsync(syncData: null, dataStream, outputStream, sheetName);
        }

        #region Internal Orchestration Facade

        /// <summary>
        /// Orchestrates the OpenXML package creation, style setup, reference sheet writing, and worksheet streaming.
        /// </summary>
        /// <typeparam name="TModel">The row model type.</typeparam>
        /// <param name="syncData">Synchronous enumerable data source, if provided.</param>
        /// <param name="asyncDataStream">Asynchronous enumerable data stream, if provided.</param>
        /// <param name="outputStream">The target output stream to write the XLSX package into.</param>
        /// <param name="sheetName">The optional worksheet name.</param>
        private async Task WriteInternalAsync<TModel>(
            IEnumerable<TModel>? syncData,
            IAsyncEnumerable<TModel>? asyncDataStream,
            Stream outputStream,
            string? sheetName) where TModel : class
        {
            var profile = GetProfile<TModel>();
            var styleManager = OpenXmlStyleManager.Create(profile);
            var rowStreamer = new OpenXmlRowStreamer<TModel>(profile, styleManager);

            using var packageBuilder = new OpenXmlPackageBuilder(outputStream);
            packageBuilder.SetStylesheet(styleManager.Stylesheet);

            // Handle dropdown columns requiring reference sheets
            var refRanges = new Dictionary<int, string>();
            var refDropdownColumns = profile.ColumnStyles
                .Where(kvp => kvp.Value.Dropdown != null && kvp.Value.Dropdown.RequiresReferenceSheet)
                .OrderBy(kvp => kvp.Key)
                .ToList();

            WorksheetPart? refWorksheetPart = null;
            if (refDropdownColumns.Count > 0)
            {
                refWorksheetPart = packageBuilder.CreateWorksheetPart();
                OpenXmlValidationReferenceSheetWriter.WriteReferenceSheet(refWorksheetPart, refDropdownColumns, refRanges);
            }

            // Stream main worksheet
            var worksheetPart = packageBuilder.CreateWorksheetPart();
            await OpenXmlWorksheetCoordinator.WriteWorksheetAsync(
                worksheetPart,
                profile,
                rowStreamer,
                syncData,
                asyncDataStream,
                refRanges);

            // Register the main worksheet FIRST so it is Sheet 1 (first sheet in the workbook)
            packageBuilder.RegisterSheet(worksheetPart, sheetName);

            // Register the validation reference sheet LAST so it is the last sheet in the workbook
            if (refWorksheetPart != null)
            {
                packageBuilder.RegisterSheet(refWorksheetPart, "_ValidationData", SheetStateValues.Hidden);
            }

            packageBuilder.Save();
        }

        /// <summary>
        /// Resolves and ensures expression compilation for the requested model's mapping profile.
        /// </summary>
        /// <typeparam name="TModel">The row model type.</typeparam>
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

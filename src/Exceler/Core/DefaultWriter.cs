using Exceler.Abstractions;
using Exceler.Configuration;
using Exceler.Pipeline.Write;
using Exceler.Pipeline.Write.Handlers;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;

namespace Exceler.Core
{
    internal class DefaultWriter : IExcelWriter
    {
        private readonly IServiceProvider _serviceProvider;

        public DefaultWriter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<byte[]> Write<TModel>(IEnumerable<TModel> data, string? sheetName = null) where TModel : class
        {
            var profile = GetProfile<TModel>();

            using var package = new ExcelPackage();
            await PopulatePackageAsync(package, profile, sheetName, data: data);

            return await package.GetAsByteArrayAsync();
        }
        public async Task WriteAsync<TModel>(IEnumerable<TModel> data, Stream outputStream, string? sheetName = null) where TModel : class
        {

            var profile = GetProfile<TModel>();

            using var package = new ExcelPackage();
            await PopulatePackageAsync(package, profile, sheetName, data: data);

            await package.SaveAsAsync(outputStream);
        }
        public async Task WriteAsync<TModel>(IAsyncEnumerable<TModel> dataStream, Stream outputStream, string? sheetName = null) where TModel : class
        {
            var profile = GetProfile<TModel>();
            using var package = new ExcelPackage();

            await PopulatePackageAsync(package, profile, sheetName, asyncData: dataStream);

            await package.SaveAsAsync(outputStream);
        }

        #region Private Methods
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

        private WriteHandler<TModel> BuildWriteChain<TModel>() where TModel : class
        {
            var head = new HeaderWriterHandler<TModel>();

            head.SetNext(new DataWriterHandler<TModel>())
                .SetNext(new StyleWriterHandler<TModel>())
                .SetNext(new FormattingWriterHandler<TModel>());

            return head;
        }
        private ExcelProfile<TModel> GetProfile<TModel>() where TModel : class
        {
            var profile = _serviceProvider.GetRequiredService<ExcelProfile<TModel>>();
            profile.EnsureBuilt();
            return profile;
        }
        #endregion
    }
}

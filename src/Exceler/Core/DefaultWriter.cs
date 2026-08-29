using Exceler.Abstractions;
using Exceler.Configuration;
using Exceler.Pipeline.Write;
using Exceler.Pipeline.Write.Handlers;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Core
{
    internal class DefaultWriter : IExcelWriter
    {
        private readonly IServiceProvider _serviceProvider;

        public DefaultWriter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<byte[]> Write<TModel>(IEnumerable<TModel> data, string? sheetName = null) where TModel : class, new()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var profile = _serviceProvider.GetRequiredService<ExcelProfile<TModel>>();
            profile.EnsureBuilt();

            using var package = new ExcelPackage();
            PopulatePackage(package, data, profile, sheetName);

            return await package.GetAsByteArrayAsync();
        }
        public async Task WriteAsync<TModel>(IEnumerable<TModel> data, Stream outputStream, string? sheetName = null) where TModel : class, new()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var profile = _serviceProvider.GetRequiredService<ExcelProfile<TModel>>();
            profile.EnsureBuilt();

            using var package = new ExcelPackage();
            PopulatePackage(package, data, profile, sheetName);

            await package.SaveAsAsync(outputStream);
        }

        #region Private Methods
        private void PopulatePackage<TModel>(ExcelPackage package, IEnumerable<TModel> data, ExcelProfile<TModel> profile, string? sheetName) where TModel : class, new()
        {
            var finalSheetName = string.IsNullOrWhiteSpace(sheetName) ? "Sheet1" : sheetName;
            var worksheet = package.Workbook.Worksheets.Add(finalSheetName);

            var context = new WriteContext<TModel>
            {
                Worksheet = worksheet,
                Profile = profile,
                Data = data
            };

            var chain = BuildWriteChain<TModel>();
            chain.Handle(context);
        }

        private WriteHandler<TModel> BuildWriteChain<TModel>() where TModel : class, new()
        {
            var head = new HeaderWriterHandler<TModel>();

            head.SetNext(new DataWriterHandler<TModel>())
                .SetNext(new StyleWriterHandler<TModel>())
                .SetNext(new FormattingWriterHandler<TModel>());

            return head;
        }
        #endregion
    }
}

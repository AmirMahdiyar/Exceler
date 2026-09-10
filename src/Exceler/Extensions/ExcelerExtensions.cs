using Exceler.Abstractions;

namespace Exceler.Extensions
{
    public static class ExcelerExtensions
    {
        public static async Task ToExcelAsync<TModel>(
            this IAsyncEnumerable<TModel> dataStream,
            IExcelWriter writer,
            Stream outputStream,
            string? sheetName = null) where TModel : class
        {
            await writer.WriteAsync(dataStream, outputStream, sheetName);
        }

        public static async Task ToExcelAsync<TModel>(
            this IEnumerable<TModel> data,
            IExcelWriter writer,
            Stream outputStream,
            string? sheetName = null) where TModel : class
        {
            await writer.WriteAsync(data, outputStream, sheetName);
        }
    }
}

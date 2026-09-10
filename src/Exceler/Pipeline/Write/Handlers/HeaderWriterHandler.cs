namespace Exceler.Pipeline.Write.Handlers
{
    /// <summary>
    /// Responsible for writing the mapped column headers to the first row of the worksheet and applying default header styling.
    /// </summary>
    internal class HeaderWriterHandler<TModel> : WriteHandler<TModel> where TModel : class
    {
        public override async Task HandleAsync(WriteContext<TModel> context)
        {
            foreach (var header in context.Profile.ColumnHeaders)
            {
                context.Worksheet.Cells[1, header.Key].Value = header.Value;
                context.Worksheet.Cells[1, header.Key].Style.Font.Bold = true;
            }

            if (Next is not null)
                await Next.HandleAsync(context);
        }
    }
}

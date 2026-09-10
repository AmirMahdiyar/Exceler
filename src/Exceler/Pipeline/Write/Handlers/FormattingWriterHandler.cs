namespace Exceler.Pipeline.Write.Handlers
{
    /// <summary>
    /// Responsible for applying final worksheet-level configurations, such as Right-To-Left view orientation and column auto-fitting.
    /// </summary>
    internal class FormattingWriterHandler<TModel> : WriteHandler<TModel> where TModel : class
    {
        public override async Task HandleAsync(WriteContext<TModel> context)
        {
            context.Worksheet.View.RightToLeft = context.Profile.RightToLeft;

            if (context.Profile.AutoFitColumns && context.Worksheet.Dimension is not null)
            {
                context.Worksheet.Cells[context.Worksheet.Dimension.Address].AutoFitColumns();
            }

            if (Next is not null)
                await Next.HandleAsync(context);
        }
    }
}

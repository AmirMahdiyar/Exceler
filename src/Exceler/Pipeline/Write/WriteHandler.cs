namespace Exceler.Pipeline.Write
{
    /// <summary>
    /// Abstract base class for the Chain of Responsibility pattern used in generating and styling Excel files.
    /// </summary>
    internal abstract class WriteHandler<TModel> where TModel : class, new()
    {
        protected WriteHandler<TModel>? Next;

        /// <summary>
        /// Sets the next handler in the document generation chain.
        /// </summary>
        /// <param name="next">The next handler to execute.</param>
        /// <returns>The provided next handler, allowing for fluent chaining.</returns>
        public WriteHandler<TModel> SetNext(WriteHandler<TModel> next)
        {
            Next = next;
            return next;
        }

        /// <summary>
        /// Executes the current document generation step and passes the context to the next handler.
        /// </summary>
        /// <param name="context">The context containing the worksheet and export data.</param>
        public abstract void Handle(WriteContext<TModel> context);
    }
}

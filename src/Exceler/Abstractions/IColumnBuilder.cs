using Exceler.Configuration;

namespace Exceler.Abstractions
{
    /// <summary>
    /// Internal contract for column mapping builders to compile expression trees and styles into an <see cref="ExcelProfile{TInput}"/>.
    /// </summary>
    /// <typeparam name="TInput">The type of the model representing an Excel row.</typeparam>
    internal interface IColumnBuilder<TInput> where TInput : class
    {
        /// <summary>
        /// Compiles the property getter/setter delegates, column styles, and header mapping into the target profile.
        /// </summary>
        /// <param name="profile">The target profile being configured.</param>
        void Compile(ExcelProfile<TInput> profile);
    }
}

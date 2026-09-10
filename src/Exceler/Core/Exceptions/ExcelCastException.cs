using System;

namespace Exceler.Core.Exceptions
{
    /// <summary>
    /// Exception thrown internally when an Excel cell value cannot be safely converted or cast to the model property's target type.
    /// Caught by the parsing handler and translated into descriptive row-level errors.
    /// </summary>
    internal class ExcelCastException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelCastException"/> class.
        /// </summary>
        public ExcelCastException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelCastException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public ExcelCastException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelCastException"/> class with a specified error message and a reference to the inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ExcelCastException(string message, Exception innerException) : base(message, innerException) { }
    }
}

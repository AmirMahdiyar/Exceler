using System;
using System.Collections.Generic;

namespace Exceler.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when the uploaded Excel worksheet headers do not match the expected headers configured in the profile.
    /// </summary>
    public class ExcelTemplateMismatchException : Exception
    {
        /// <summary>
        /// Gets the list of missing, mismatched, or unexpected header error descriptions.
        /// </summary>
        public List<string> MissingOrInvalidHeaders { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelTemplateMismatchException"/> class with a collection of header validation error messages.
        /// </summary>
        /// <param name="errors">The list of missing, unexpected, or invalid header descriptions.</param>
        public ExcelTemplateMismatchException(List<string> errors)
            : base("Template Is Not Matched With Standard Template")
        {
            MissingOrInvalidHeaders = errors ?? new List<string>();
        }
    }
}

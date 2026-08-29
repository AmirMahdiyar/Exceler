using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.Core.Exceptions
{
    public class ExcelTemplateMismatchException : Exception
    {
        public List<string> MissingOrInvalidHeaders { get; }

        public ExcelTemplateMismatchException(List<string> errors)
            : base("Template Is Not Matched With Standard Template")
        {
            MissingOrInvalidHeaders = errors;
        }
    }
}

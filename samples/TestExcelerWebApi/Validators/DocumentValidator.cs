using Exceler.Abstractions;
using TestExcelerWebApi.Inputs;

namespace TestExcelerWebApi.Validators
{
    public class DocumentValidator : IExcelValidator<DocumentExcelInput>
    {
        public IEnumerable<string> Validate(DocumentExcelInput input)
        {
            var errors = new List<string>();

            if (input.Id < 0)
                errors.Add("Id Must be Positive");

            return errors;
        }
    }
}

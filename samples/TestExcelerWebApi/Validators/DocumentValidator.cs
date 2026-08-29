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
                errors.Add("شناسه سند باید عدد مثبت باشد.");

            return errors;
        }
    }
}

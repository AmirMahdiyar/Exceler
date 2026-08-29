using Exceler.Abstractions;
using TestExcelerWebApi.Inputs;

namespace TestExcelerWebApi.Validators
{
    public class EmployeeValidator : IExcelValidator<EmployeeExcelInput>
    {
        public IEnumerable<string> Validate(EmployeeExcelInput input)
        {
            var errors = new List<string>();

            if (input.Id <= 0)
                errors.Add("EmployeeId Must be Positive");

            if (string.IsNullOrWhiteSpace(input.FullName))
                errors.Add("Fullname Can't be Null Or Empty");

            return errors;
        }
    }
}

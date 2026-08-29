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
                errors.Add("شناسه پرسنل باید یک عدد مثبت باشد.");

            if (string.IsNullOrWhiteSpace(input.FullName))
                errors.Add("نام و نام‌خانوادگی نمی‌تواند خالی باشد.");

            return errors;
        }
    }
}

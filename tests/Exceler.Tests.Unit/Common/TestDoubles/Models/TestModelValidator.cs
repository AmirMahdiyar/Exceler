using Exceler.Abstractions;

namespace Exceler.Tests.Common.TestDoubles.Models
{
    public class TestModelValidator : IExcelValidator<TestModel>
    {
        public IEnumerable<string> Validate(TestModel input)
        {
            if (input.Balance < 0)
                yield return "Balance cannot be negative.";
        }
    }
}

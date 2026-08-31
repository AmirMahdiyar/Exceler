using Exceler.Abstractions;

namespace Exceler.Tests.Infrastructure.ModelOfTest
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

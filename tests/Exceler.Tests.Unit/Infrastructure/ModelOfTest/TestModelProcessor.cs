using Exceler.Abstractions;

namespace Exceler.Tests.Infrastructure.ModelOfTest
{
    public class TestModelProcessor : IExcelProcessor<TestModel, TestModel>
    {
        public TestModel Process(TestModel input)
            => input;
    }
}

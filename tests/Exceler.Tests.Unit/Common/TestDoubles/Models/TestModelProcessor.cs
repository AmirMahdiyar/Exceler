using Exceler.Abstractions;

namespace Exceler.Tests.Common.TestDoubles.Models
{
    public class TestModelProcessor : IExcelProcessor<TestModel, TestModel>
    {
        public TestModel Process(TestModel input)
            => input;
    }
}

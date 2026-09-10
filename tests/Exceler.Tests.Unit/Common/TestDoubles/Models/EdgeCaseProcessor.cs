using Exceler.Abstractions;

namespace Exceler.Tests.Common.TestDoubles.Models
{
    public class EdgeCaseProcessor : IExcelProcessor<EdgeCaseModel, EdgeCaseModel>
    {
        public EdgeCaseModel Process(EdgeCaseModel input)
        {
            return input;
        }
    }
}

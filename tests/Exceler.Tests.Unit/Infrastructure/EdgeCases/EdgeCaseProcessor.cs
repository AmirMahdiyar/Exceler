using Exceler.Abstractions;

namespace Exceler.Tests.Infrastructure.EdgeCases
{
    public class EdgeCaseProcessor : IExcelProcessor<EdgeCaseModel, EdgeCaseModel>
    {
        public EdgeCaseModel Process(EdgeCaseModel input)
        {
            return input;
        }
    }
}

using Exceler.Configuration;

namespace Exceler.Tests.Infrastructure.EdgeCases
{
    public class EdgeCaseProfile : ExcelProfile<EdgeCaseModel>
    {
        public EdgeCaseProfile()
        {
            Map(x => x.NullableInt).ToColumn(1).WithHeader("Nullable Int");
            Map(x => x.IsActive).ToColumn(2).WithHeader("Is Active");
            Map(x => x.Status).ToColumn(3).WithHeader("Status");
            Map(x => x.Identifier).ToColumn(4).WithHeader("Identifier");
            Map(x => x.Ratio).ToColumn(5).WithHeader("Ratio");
        }
    }
}

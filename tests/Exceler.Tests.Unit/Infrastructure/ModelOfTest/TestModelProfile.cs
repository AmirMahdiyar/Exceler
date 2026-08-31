using Exceler.Configuration;

namespace Exceler.Tests.Infrastructure.ModelOfTest
{
    public class TestModelProfile : ExcelProfile<TestModel>
    {
        public TestModelProfile()
        {
            Map(x => x.Id).ToColumn(1).WithHeader("User ID");
            Map(x => x.FullName).ToColumn(2).WithHeader("Full Name").IsBold();
            Map(x => x.Balance).ToColumn(3).WithHeader("Account Balance");
            Map(x => x.CreatedAt).ToColumn(4).WithHeader("Creation Date");
        }
    }
}

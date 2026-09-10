using AutoFixture;

namespace Exceler.Tests.Common.TestDoubles.Models
{
    public class TestDataFactory
    {
        private static readonly Fixture _fixture = new Fixture();

        public static TestModel CreateValid()
            => new TestModelBuilder().Build();

        public static List<TestModel> CreateMany(int count = 10000)
            => _fixture.CreateMany<TestModel>(count).ToList();
    }
}

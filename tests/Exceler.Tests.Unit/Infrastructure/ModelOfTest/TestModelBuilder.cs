namespace Exceler.Tests.Infrastructure.ModelOfTest
{
    public class TestModelBuilder
    {
        private int _id = 1;
        private string _fullName = "Default Name";
        private decimal _balance = 1000m;
        private DateTime _createdAt = new DateTime(2026, 1, 1);

        public TestModelBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        public TestModelBuilder WithFullName(string fullName)
        {
            _fullName = fullName;
            return this;
        }

        public TestModel Build()
        {
            return new TestModel
            {
                Id = _id,
                FullName = _fullName,
                Balance = _balance,
                CreatedAt = _createdAt
            };
        }
    }
}

namespace Exceler.Tests.Common.TestDoubles.Models
{
    public class EdgeCaseModel
    {
        public int? NullableInt { get; set; }
        public bool IsActive { get; set; }
        public TestStatus Status { get; set; }
        public Guid Identifier { get; set; }
        public double Ratio { get; set; }
    }
}

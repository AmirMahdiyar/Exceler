namespace TestExcelerWebApi.Inputs
{
    public record EmployeeExcelInput
    {
        public int Id { get; init; }
        public string FullName { get; init; }
        public string HireDate { get; init; }
    }
}

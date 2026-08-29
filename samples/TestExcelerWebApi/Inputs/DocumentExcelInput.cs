namespace TestExcelerWebApi.Inputs
{
    public record DocumentExcelInput
    {
        public int? Id { get; init; }
        public string Type { get; init; }
        public string Reference { get; init; }
        public string Warehouse { get; init; }
        public string Destination { get; init; }
        public string AccountParty { get; init; }
        public string DetailCode { get; init; }
        public string Number { get; init; }
        public string Date { get; init; }
        public string Receiver { get; init; }
        public string Description { get; init; }
        public string Status { get; init; }
    }
}

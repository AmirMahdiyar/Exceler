namespace TestExcelerWebApi.Dtos
{
    public record DocumentDto
    {
        public int? DocumentId { get; init; }
        public string DocumentType { get; init; }
        public string WarehouseName { get; init; }
        public string Date { get; init; }
        public string Status { get; init; }
        public string FullDescription { get; init; }
    }
}

namespace TestExcelerWebApi.Inputs
{
    public class InvoiceExcelInput
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string DlNumber { get; set; }
        public string Deliverer { get; set; }
        public DateTime DeliveredTime { get; set; }
    }
}

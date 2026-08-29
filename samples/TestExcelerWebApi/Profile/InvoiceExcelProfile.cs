using Exceler.Configuration;
using TestExcelerWebApi.Inputs;

namespace TestExcelerWebApi.Profile
{
    public class InvoiceExcelProfile : ExcelProfile<InvoiceExcelInput>
    {
        public InvoiceExcelProfile()
        {
            Map(x => x.Id)
                .ToColumn(1)
                .WithHeader("Id")
                .IsBold(true)
                .WithFontColor(System.Drawing.Color.Black);

            Map(x => x.Type)
                .ToColumn(2)
                .WithHeader("Type")
                .IsBold(false)
                .WithBackgroundColor(System.Drawing.Color.Blue);

            Map(x => x.DlNumber)
                .ToColumn(3)
                .WithHeader("DlNumber")
                .IsBold(false)
                .WithFontColor(System.Drawing.Color.Beige);

            Map(x => x.Deliverer)
                .ToColumn(4)
                .WithHeader("Deliverer")
                .IsBold(true);

            Map(x => x.DeliveredTime)
                .ToColumn(5)
                .WithHeader("DeliveredTime");
        }
    }
}

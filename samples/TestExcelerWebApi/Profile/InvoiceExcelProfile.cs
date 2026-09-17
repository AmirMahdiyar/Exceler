using Exceler.Configuration;
using TestExcelerWebApi.Inputs;

namespace TestExcelerWebApi.Profile
{
    public class InvoiceExcelProfile : ExcelProfile<InvoiceExcelInput>
    {
        public InvoiceExcelProfile()
        {
            // Conditional Style 1: Row-level styling for EPPlus
            // Highlights the entire invoice row with soft yellow when delivered by Head Courier
            WithConditionalRowStyle(
                x => x.Deliverer == "Mohammad, Head Courier",
                s => s.WithBackgroundColor(ExcelColor.SoftYellow)
            );

            Map(x => x.Id)
                .ToColumn(1)
                .WithHeader("Id")
                .IsBold(true)
                .WithFontColor(System.Drawing.Color.Black);

            // Smart Dropdown 1: Strongly-typed enum list for EPPlus
            // Conditional Style 2: Column-level styling overriding row styling
            Map(x => x.Type)
                .ToColumn(2)
                .WithHeader("Type")
                .IsBold(false)
                .WithBackgroundColor(System.Drawing.Color.LightBlue)
                .WithDropdownFromEnum<InvoiceType>()
                .WithConditionalStyle(
                    x => x.Type == nameof(InvoiceType.Commercial),
                    s => s.WithBackgroundColor(ExcelColor.SoftGreen).SetBold(true)
                )
                .WithConditionalStyle(
                    x => x.Type == nameof(InvoiceType.CreditNote),
                    s => s.WithBackgroundColor(ExcelColor.SoftRed).WithFontColor(ExcelColor.DarkRed).SetBold(true)
                );

            Map(x => x.DlNumber)
                .ToColumn(3)
                .WithHeader("DlNumber")
                .IsBold(false)
                .WithFontColor(System.Drawing.Color.Beige);

            // Smart Dropdown 2: String array with comma handling in EPPlus engine
            // Notice: "Mohammad, Head Courier" contains a comma, automatically handled via hidden reference sheet.
            Map(x => x.Deliverer)
                .ToColumn(4)
                .WithHeader("Deliverer")
                .IsBold(true)
                .WithDropdown("Ali", "Reza", "Mohammad, Head Courier", "Asghar", "Akbar");

            Map(x => x.DeliveredTime)
                .ToColumn(5)
                .WithHeader("DeliveredTime");
        }
    }
}

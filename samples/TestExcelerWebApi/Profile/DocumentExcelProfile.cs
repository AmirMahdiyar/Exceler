using Exceler.Configuration;
using TestExcelerWebApi.Inputs;

namespace TestExcelerWebApi.Profile
{
    public class DocumentExcelProfile : ExcelProfile<DocumentExcelInput>
    {
        public DocumentExcelProfile()
        {
            Map(x => x.Id).ToColumn(1).WithHeader("Id");

            // Smart Dropdown: Document Category List
            Map(x => x.Type)
                .ToColumn(2)
                .WithHeader("Type")
                .WithDropdown("Receipt", "Invoice", "Return", "Transfer", "Adjustment");

            Map(x => x.Reference).ToColumn(3).WithHeader("Foundation");

            // Smart Dropdown: Warehouse Location (Options with commas route automatically to hidden sheet)
            Map(x => x.Warehouse)
                .ToColumn(4)
                .WithHeader("Inventory")
                .WithDropdown("Central Hub", "North Warehouse", "South Distribution, Unit 2", "East Annex");

            Map(x => x.Destination).ToColumn(5).WithHeader("Headed Inventory");
            Map(x => x.AccountParty).ToColumn(6).WithHeader("Party");
            Map(x => x.DetailCode).ToColumn(7).WithHeader("DlCode");
            Map(x => x.Number).ToColumn(8).WithHeader("Number");
            Map(x => x.Date).ToColumn(9).WithHeader("Date");
            Map(x => x.Receiver).ToColumn(10).WithHeader("Receiver");
            Map(x => x.Description).ToColumn(12).WithHeader("Description");

            // Smart Dropdown: Workflow Status
            Map(x => x.Status)
                .ToColumn(13)
                .WithHeader("Status")
                .WithDropdown("Draft", "Pending Approval", "Approved", "Completed", "Archived");
        }
    }
}

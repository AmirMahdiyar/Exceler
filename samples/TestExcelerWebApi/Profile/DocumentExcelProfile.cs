using Exceler.Configuration;
using TestExcelerWebApi.Inputs;

namespace TestExcelerWebApi.Profile
{
    public class DocumentExcelProfile : ExcelProfile<DocumentExcelInput>
    {
        public DocumentExcelProfile()
        {
            Map(x => x.Id).ToColumn(1).WithHeader("شناسه");
            Map(x => x.Type).ToColumn(2).WithHeader("نوع");
            Map(x => x.Reference).ToColumn(3).WithHeader("مبنا");
            Map(x => x.Warehouse).ToColumn(4).WithHeader("انبار");
            Map(x => x.Destination).ToColumn(5).WithHeader("انبار مقصد");
            Map(x => x.AccountParty).ToColumn(6).WithHeader("طرف حساب");
            Map(x => x.DetailCode).ToColumn(7).WithHeader("کد تفصیل");
            Map(x => x.Number).ToColumn(8).WithHeader("شماره");
            Map(x => x.Date).ToColumn(9).WithHeader("تاریخ");
            Map(x => x.Receiver).ToColumn(10).WithHeader("گیرنده");
            Map(x => x.Description).ToColumn(12).WithHeader("توضیحات");
            Map(x => x.Status).ToColumn(13).WithHeader("وضعیت");
        }
    }
}

using Exceler.Configuration;
using TestExcelerWebApi.Converter;
using TestExcelerWebApi.Inputs;

namespace TestExcelerWebApi.Profile
{
    public class EmployeeExcelProfile : ExcelProfile<EmployeeExcelInput>
    {
        public EmployeeExcelProfile()
        {
            Map(x => x.Id).ToColumn(1).WithHeader("کد");
            Map(x => x.FullName).ToColumn(2).WithHeader("نام");
            Map(x => x.HireDate)
                .ToColumn(3)
                .WithHeader("تاریخ")
                .WithConverter(new PersianDateConverter());
        }
    }
}

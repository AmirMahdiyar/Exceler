using Exceler.Configuration;
using TestExcelerWebApi.Converter;
using TestExcelerWebApi.Inputs;

namespace TestExcelerWebApi.Profile
{
    public class EmployeeExcelProfile : ExcelProfile<EmployeeExcelInput>
    {
        public EmployeeExcelProfile()
        {
            Map(x => x.Id).ToColumn(1).WithHeader("Id");
            Map(x => x.FullName).ToColumn(2).WithHeader("Name");
            Map(x => x.HireDate)
                .ToColumn(3)
                .WithHeader("Date")
                .WithConverter(new PersianDateConverter());
        }
    }
}

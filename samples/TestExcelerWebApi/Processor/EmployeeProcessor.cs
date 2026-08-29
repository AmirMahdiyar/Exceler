using Exceler.Abstractions;
using TestExcelerWebApi.Dtos;
using TestExcelerWebApi.Inputs;

namespace TestExcelerWebApi.Processor
{
    public class EmployeeProcessor : IExcelProcessor<EmployeeExcelInput, EmployeeDto>
    {
        public EmployeeDto Process(EmployeeExcelInput input)
        {
            return new EmployeeDto(input.Id, input.FullName, input.HireDate);
        }
    }
}

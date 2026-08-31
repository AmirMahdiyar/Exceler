using Exceler.Abstractions;
using Exceler.DependencyInjection;
using Exceler.Tests.Infrastructure.ModelOfTest;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;

namespace Exceler.Tests.Infrastructure.Base
{
    public abstract class ExcelerTestBase
    {
        protected readonly IExcelReader Reader;
        protected readonly IExcelWriter Writer;
        protected readonly IServiceProvider ServiceProvider;

        protected ExcelerTestBase()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var services = new ServiceCollection();

            services.AddExcelCore(builder =>
                builder.RegisterFromAssemblyContaining<TestModelProfile>());

            services.AddScoped<IExcelValidator<TestModel>, TestModelValidator>();

            ServiceProvider = services.BuildServiceProvider();
            Reader = ServiceProvider.GetRequiredService<IExcelReader>();
            Writer = ServiceProvider.GetRequiredService<IExcelWriter>();
        }
    }
}

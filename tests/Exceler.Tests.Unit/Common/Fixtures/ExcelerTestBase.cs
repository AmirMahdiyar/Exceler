using Exceler.Abstractions;
using Exceler.DependencyInjection;
using Exceler.Tests.Common.TestDoubles.Models;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;

namespace Exceler.Tests.Common.Fixtures
{
    public abstract class ExcelerTestBase
    {
        protected readonly IExcelReader Reader;
        protected readonly IExcelWriter Writer;
        protected readonly IServiceProvider ServiceProvider;

        protected ExcelerTestBase()
        {
            (Reader, Writer, ServiceProvider) = ExcelerTestBed.CreateDefaultSuite();
        }
    }
}

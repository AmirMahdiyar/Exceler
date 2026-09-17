using Exceler.Abstractions;
using Exceler.Configuration;
using Exceler.Core;
using Exceler.Core.EPPlus;
using Exceler.Core.OpenXml;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Exceler.DependencyInjection
{
    /// <summary>
    /// Provides extension methods for registering the Exceler framework components into the DI container.
    /// </summary>
    public static class ExcelerServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the core Exceler framework components (<see cref="IExcelReader"/> and <see cref="IExcelWriter"/>)
        /// and configures profiles, processors, validators, engine selection, and licensing via the builder callback.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configure">An optional delegate to configure Exceler options, engine, license context, and scan assemblies.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddExcelCore(this IServiceCollection services, Action<IExcelerBuilder>? configure = null)
        {
            services.AddScoped<IExcelReader, DefaultReader>();

            var builder = new ExcelerBuilder(services);

            if (configure != null)
            {
                configure(builder);
            }
            else
            {
                builder.RegisterFromAssembly(Assembly.GetCallingAssembly());
            }

            services.AddSingleton(new ExcelerOptions { IsLicenseConfigured = builder.IsLicenseConfigured });

            if (builder.SelectedEngine == ExcelerEngine.OpenXml)
            {
                services.AddScoped<IExcelWriter, OpenXmlWriterEngine>();
            }
            else
            {
                services.AddScoped<IExcelWriter, EPPlusWriterEngine>();

                if (!builder.IsLicenseConfigured)
                {
                    throw new InvalidOperationException(
                        "When using the EPPlus engine, you MUST explicitly accept the license terms " +
                        "by calling either '.UseNonCommercialLicense()' or '.UseCommercialLicense()' " +
                        "inside the AddExcelCore configuration builder.");
                }
            }

            return services;
        }
    }
}

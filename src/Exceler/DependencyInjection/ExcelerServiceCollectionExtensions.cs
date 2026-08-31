using Exceler.Abstractions;
using Exceler.Core;
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
        /// Scans the provided assemblies and automatically registers all Excel profiles, processors, and validators.
        /// Also registers the core <see cref="IExcelReader"/> and <see cref="IExcelWriter"/> services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="assemblies">The assemblies to scan. If none are provided, the calling assembly is scanned.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddExcelCore(this IServiceCollection services, Action<IExcelerBuilder>? configure = null)
        {
            services.AddScoped<IExcelReader, DefaultReader>();
            services.AddScoped<IExcelWriter, DefaultWriter>();

            var builder = new ExcelerBuilder(services);

            if (configure != null)
            {
                configure(builder);
            }
            else
            {
                builder.RegisterFromAssembly(Assembly.GetCallingAssembly());
            }
            if (!builder.IsLicenseConfigured)
            {
                throw new InvalidOperationException(
                    "Exceler uses EPPlus under the hood. You MUST explicitly accept the license terms " +
                    "by calling either '.UseNonCommercialLicense()' or '.UseCommercialLicense()' " +
                    "inside the AddExcelCore configuration builder.");
            }

            return services;
        }
    }
}

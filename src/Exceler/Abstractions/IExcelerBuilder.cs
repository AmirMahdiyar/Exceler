using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using System.ComponentModel;
using System.Reflection;

namespace Exceler.Abstractions
{
    /// <summary>
    /// Provides a builder interface for configuring the Exceler framework components.
    /// </summary>
    public interface IExcelerBuilder
    {
        /// <summary>
        /// Gets the underlying service collection to allow advanced custom registrations.
        /// </summary>
        IServiceCollection Services { get; }
        /// <summary>
        /// Gets a value indicating whether the EPPlus license has been explicitly configured.
        /// </summary>
        /// <remarks>This property is used internally by the framework. It is hidden from IntelliSense.</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool IsLicenseConfigured { get; }

        /// <summary>
        /// Gets the configured EPPlus license context, if one has been set.
        /// </summary>
        OfficeOpenXml.LicenseContext? LicenseContext { get; }


        /// <summary>
        /// Scans the specified assembly and automatically registers all discovered profiles, processors, and validators.
        /// </summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <returns>The current <see cref="IExcelerBuilder"/> instance for chaining.</returns>
        IExcelerBuilder RegisterFromAssembly(Assembly assembly);

        /// <summary>
        /// Scans the assembly containing the specified type and automatically registers all discovered components.
        /// </summary>
        /// <typeparam name="T">A type contained within the target assembly.</typeparam>
        /// <returns>The current <see cref="IExcelerBuilder"/> instance for chaining.</returns>
        IExcelerBuilder RegisterFromAssemblyContaining<T>();
        /// <summary>
        /// Gets the currently selected spreadsheet generation engine.
        /// Defaults to <see cref="ExcelerEngine.OpenXml"/>.
        /// </summary>
        ExcelerEngine SelectedEngine { get; }

        /// <summary>
        /// Configures the Exceler framework to use the ultra-fast, zero-allocation SAX-based OpenXML engine.
        /// This is the default engine in Exceler v2.0.0 and does not require an EPPlus license.
        /// </summary>
        /// <returns>The current <see cref="IExcelerBuilder"/> instance for chaining.</returns>
        IExcelerBuilder UseOpenXmlEngine();

        /// <summary>
        /// Configures the Exceler framework to use the EPPlus DOM-based engine.
        /// Required if you need DOM-dependent features like runtime font-metric column auto-fitting (AutoFitColumns).
        /// When using EPPlus, you MUST configure an EPPlus license via <see cref="UseNonCommercialLicense"/> or <see cref="UseCommercialLicense"/>.
        /// </summary>
        /// <returns>The current <see cref="IExcelerBuilder"/> instance for chaining.</returns>
        IExcelerBuilder UseEPPlusEngine();

        /// <summary>
        /// Configures the Exceler framework to use the EPPlus Non-Commercial license.
        /// </summary>
        /// <remarks>
        /// Call this method ONLY if your project qualifies for the PolyForm Noncommercial license 
        /// (e.g., personal, educational, or open-source non-profit projects).
        /// </remarks>
        /// <returns>The current <see cref="IExcelerBuilder"/> instance for chaining.</returns>
        IExcelerBuilder UseNonCommercialLicense();

        /// <summary>
        /// Configures the Exceler framework to use the EPPlus Commercial license.
        /// </summary>
        /// <remarks>
        /// Call this method if you or your company have purchased a commercial license for EPPlus 
        /// and are using this package in a corporate or profit-generating environment.
        /// </remarks>
        /// <returns>The current <see cref="IExcelerBuilder"/> instance for chaining.</returns>
        IExcelerBuilder UseCommercialLicense();
    }
}

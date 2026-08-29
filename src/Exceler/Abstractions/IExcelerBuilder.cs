using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
    }
}

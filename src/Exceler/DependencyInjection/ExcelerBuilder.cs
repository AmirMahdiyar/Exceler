using Exceler.Abstractions;
using Exceler.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using System.Reflection;

namespace Exceler.DependencyInjection
{
    /// <summary>
    /// Internal implementation of the <see cref="IExcelerBuilder"/> for configuring DI services.
    /// </summary>
    internal class ExcelerBuilder : IExcelerBuilder
    {
        /// <inheritdoc />
        public IServiceCollection Services { get; }

        /// <inheritdoc />
        public bool IsLicenseConfigured { get; private set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelerBuilder"/> class.
        /// </summary>
        /// <param name="services">The service collection to populate.</param>
        /// <exception cref="ArgumentNullException">Thrown if services is null.</exception>
        public ExcelerBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <inheritdoc />
        public IExcelerBuilder RegisterFromAssemblyContaining<T>()
        {
            return RegisterFromAssembly(typeof(T).Assembly);
        }

        /// <inheritdoc />
        public IExcelerBuilder RegisterFromAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            var concreteTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .ToList();

            foreach (var type in concreteTypes)
            {
                RegisterProfile(type);
                RegisterProcessor(type);
                RegisterValidator(type);
            }

            return this;
        }

        /// <inheritdoc />
        public IExcelerBuilder UseNonCommercialLicense()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            IsLicenseConfigured = true;
            return this;
        }

        /// <inheritdoc />
        public IExcelerBuilder UseCommercialLicense()
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            IsLicenseConfigured = true;
            return this;
        }

        #region Private Methods

        /// <summary>
        /// Registers a class inheriting from <see cref="ExcelProfile{TTarget}"/> into the service collection.
        /// </summary>
        /// <param name="type">The type to check and register.</param>
        private void RegisterProfile(Type type)
        {
            var profileBaseType = GetBaseTypeOfRawGeneric(type, typeof(ExcelProfile<>));
            if (profileBaseType != null)
            {
                Services.AddSingleton(profileBaseType, type);
            }
        }

        /// <summary>
        /// Registers a class implementing <see cref="IExcelProcessor{TInput, TOutput}"/> into the service collection.
        /// </summary>
        /// <param name="type">The type to check and register.</param>
        private void RegisterProcessor(Type type)
        {
            var processorInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IExcelProcessor<,>));

            foreach (var pInterface in processorInterfaces)
            {
                Services.AddScoped(pInterface, type);
            }
        }

        /// <summary>
        /// Registers a class implementing <see cref="IExcelValidator{TInput}"/> into the service collection.
        /// </summary>
        /// <param name="type">The type to check and register.</param>
        private void RegisterValidator(Type type)
        {
            var validatorInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IExcelValidator<>));

            foreach (var vInterface in validatorInterfaces)
            {
                Services.AddScoped(vInterface, type);
            }
        }

        /// <summary>
        /// Finds the constructed generic base type of a given raw generic type definition.
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <param name="genericType">The raw generic base type definition to match.</param>
        /// <returns>The constructed generic type if found; otherwise, null.</returns>
        private static Type? GetBaseTypeOfRawGeneric(Type type, Type genericType)
        {
            var currentType = type.BaseType;
            while (currentType != null && currentType != typeof(object))
            {
                if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == genericType)
                    return currentType;

                currentType = currentType.BaseType;
            }
            return null;
        }
        #endregion
    }
}

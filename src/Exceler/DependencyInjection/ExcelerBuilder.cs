using Exceler.Abstractions;
using Exceler.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Exceler.DependencyInjection
{
    /// <summary>
    /// Internal implementation of the <see cref="IExcelerBuilder"/> for configuring DI services.
    /// </summary>
    internal class ExcelerBuilder : IExcelerBuilder
    {
        public IServiceCollection Services { get; }

        public ExcelerBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IExcelerBuilder RegisterFromAssemblyContaining<T>()
        {
            return RegisterFromAssembly(typeof(T).Assembly);
        }

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

        #region Private Methods
        private void RegisterProfile(Type type)
        {
            var profileBaseType = GetBaseTypeOfRawGeneric(type, typeof(ExcelProfile<>));
            if (profileBaseType != null)
            {
                Services.AddSingleton(profileBaseType, type);
            }
        }

        private void RegisterProcessor(Type type)
        {
            var processorInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IExcelProcessor<,>));

            foreach (var pInterface in processorInterfaces)
            { 
                Services.AddScoped(pInterface, type);
            }
        }

        private void RegisterValidator(Type type)
        {
            var validatorInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IExcelValidator<>));

            foreach (var vInterface in validatorInterfaces)
            {
                Services.AddScoped(vInterface, type);
            }
        }

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

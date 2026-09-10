using Exceler.Abstractions;
using Exceler.Configuration;
using Exceler.Core;
using Exceler.DependencyInjection;
using Exceler.Tests.Common.TestDoubles.Models;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using System;

namespace Exceler.Tests.Common.Fixtures
{
    /// <summary>
    /// Test Bed and Object Mother pattern providing fluent builder and static shortcuts
    /// for configuring and instantiating <see cref="IExcelReader"/> and <see cref="IExcelWriter"/>
    /// without polluting unit test Arrange phases with DI container mechanics.
    /// </summary>
    public class ExcelerTestBed
    {
        private readonly IServiceCollection _services;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelerTestBed"/> class.
        /// Automatically configures the non-commercial EPPlus license context and registers core services.
        /// </summary>
        public ExcelerTestBed()
        {
            _services = new ServiceCollection();
            _services.AddScoped<IExcelReader, DefaultReader>();
            _services.AddScoped<IExcelWriter, DefaultWriter>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Creates a new fluent configuration builder for the test bed.
        /// </summary>
        /// <returns>A new <see cref="ExcelerTestBed"/> builder instance.</returns>
        public static ExcelerTestBed Configure() => new();

        /// <summary>
        /// Registers a mapping profile type into the test bed.
        /// </summary>
        /// <typeparam name="TModel">The target model type.</typeparam>
        /// <typeparam name="TProfile">The profile type implementing <see cref="ExcelProfile{TModel}"/>.</typeparam>
        /// <returns>The builder instance for fluent chaining.</returns>
        public ExcelerTestBed WithProfile<TModel, TProfile>() 
            where TModel : class 
            where TProfile : ExcelProfile<TModel>, new()
        {
            _services.AddSingleton<ExcelProfile<TModel>, TProfile>();
            return this;
        }

        /// <summary>
        /// Registers a specific instance of a mapping profile into the test bed.
        /// </summary>
        /// <typeparam name="TModel">The target model type.</typeparam>
        /// <param name="profile">The profile instance to register.</param>
        /// <returns>The builder instance for fluent chaining.</returns>
        public ExcelerTestBed WithProfile<TModel>(ExcelProfile<TModel> profile) where TModel : class
        {
            _services.AddSingleton(profile);
            return this;
        }

        /// <summary>
        /// Registers a synchronous row validator into the test bed.
        /// </summary>
        /// <typeparam name="TModel">The target model type.</typeparam>
        /// <typeparam name="TValidator">The validator type implementing <see cref="IExcelValidator{TModel}"/>.</typeparam>
        /// <returns>The builder instance for fluent chaining.</returns>
        public ExcelerTestBed WithValidator<TModel, TValidator>() 
            where TModel : class 
            where TValidator : class, IExcelValidator<TModel>
        {
            _services.AddScoped<IExcelValidator<TModel>, TValidator>();
            return this;
        }

        /// <summary>
        /// Registers an asynchronous row validator into the test bed.
        /// </summary>
        /// <typeparam name="TModel">The target model type.</typeparam>
        /// <typeparam name="TValidator">The validator type implementing <see cref="IAsyncExcelValidator{TModel}"/>.</typeparam>
        /// <returns>The builder instance for fluent chaining.</returns>
        public ExcelerTestBed WithAsyncValidator<TModel, TValidator>() 
            where TModel : class 
            where TValidator : class, IAsyncExcelValidator<TModel>
        {
            _services.AddScoped<IAsyncExcelValidator<TModel>, TValidator>();
            return this;
        }

        /// <summary>
        /// Registers a synchronous row processor into the test bed.
        /// </summary>
        /// <typeparam name="TInput">The input model type.</typeparam>
        /// <typeparam name="TOutput">The output model type.</typeparam>
        /// <typeparam name="TProcessor">The processor type implementing <see cref="IExcelProcessor{TInput, TOutput}"/>.</typeparam>
        /// <returns>The builder instance for fluent chaining.</returns>
        public ExcelerTestBed WithProcessor<TInput, TOutput, TProcessor>() 
            where TInput : class 
            where TProcessor : class, IExcelProcessor<TInput, TOutput>
        {
            _services.AddScoped<IExcelProcessor<TInput, TOutput>, TProcessor>();
            return this;
        }

        /// <summary>
        /// Registers an asynchronous row processor into the test bed.
        /// </summary>
        /// <typeparam name="TInput">The input model type.</typeparam>
        /// <typeparam name="TOutput">The output model type.</typeparam>
        /// <typeparam name="TProcessor">The processor type implementing <see cref="IAsyncExcelProcessor{TInput, TOutput}"/>.</typeparam>
        /// <returns>The builder instance for fluent chaining.</returns>
        public ExcelerTestBed WithAsyncProcessor<TInput, TOutput, TProcessor>() 
            where TInput : class 
            where TProcessor : class, IAsyncExcelProcessor<TInput, TOutput>
        {
            _services.AddScoped<IAsyncExcelProcessor<TInput, TOutput>, TProcessor>();
            return this;
        }

        /// <summary>
        /// Scans an assembly containing the specified marker type for profiles, validators, and processors.
        /// </summary>
        /// <typeparam name="TMarker">A type located within the target assembly.</typeparam>
        /// <returns>The builder instance for fluent chaining.</returns>
        public ExcelerTestBed WithAssemblyScanningContaining<TMarker>()
        {
            _services.AddExcelCore(builder =>
            {
                builder.RegisterFromAssemblyContaining<TMarker>();
                builder.UseNonCommercialLicense();
            });
            return this;
        }

        /// <summary>
        /// Builds and returns the underlying configured <see cref="IServiceProvider"/>.
        /// </summary>
        /// <returns>The built service provider.</returns>
        public IServiceProvider BuildProvider() => _services.BuildServiceProvider();

        /// <summary>
        /// Builds and resolves an <see cref="IExcelReader"/> instance.
        /// </summary>
        /// <returns>A configured reader instance.</returns>
        public IExcelReader BuildReader() => BuildProvider().GetRequiredService<IExcelReader>();

        /// <summary>
        /// Builds and resolves an <see cref="IExcelWriter"/> instance.
        /// </summary>
        /// <returns>A configured writer instance.</returns>
        public IExcelWriter BuildWriter() => BuildProvider().GetRequiredService<IExcelWriter>();

        #region Object Mother Static Shortcuts

        /// <summary>
        /// Creates an <see cref="IExcelReader"/> configured with a single mapping profile type.
        /// </summary>
        /// <typeparam name="TModel">The target model type.</typeparam>
        /// <typeparam name="TProfile">The profile type.</typeparam>
        /// <returns>A pre-configured <see cref="IExcelReader"/>.</returns>
        public static IExcelReader CreateReader<TModel, TProfile>() 
            where TModel : class 
            where TProfile : ExcelProfile<TModel>, new()
            => Configure().WithProfile<TModel, TProfile>().BuildReader();

        /// <summary>
        /// Creates an <see cref="IExcelReader"/> configured with a specific profile instance.
        /// </summary>
        /// <typeparam name="TModel">The target model type.</typeparam>
        /// <param name="profile">The profile instance.</param>
        /// <returns>A pre-configured <see cref="IExcelReader"/>.</returns>
        public static IExcelReader CreateReader<TModel>(ExcelProfile<TModel> profile) where TModel : class
            => Configure().WithProfile(profile).BuildReader();

        /// <summary>
        /// Creates an <see cref="IExcelWriter"/> configured with a single mapping profile type.
        /// </summary>
        /// <typeparam name="TModel">The target model type.</typeparam>
        /// <typeparam name="TProfile">The profile type.</typeparam>
        /// <returns>A pre-configured <see cref="IExcelWriter"/>.</returns>
        public static IExcelWriter CreateWriter<TModel, TProfile>() 
            where TModel : class 
            where TProfile : ExcelProfile<TModel>, new()
            => Configure().WithProfile<TModel, TProfile>().BuildWriter();

        /// <summary>
        /// Creates an <see cref="IExcelWriter"/> configured with a specific profile instance.
        /// </summary>
        /// <typeparam name="TModel">The target model type.</typeparam>
        /// <param name="profile">The profile instance.</param>
        /// <returns>A pre-configured <see cref="IExcelWriter"/>.</returns>
        public static IExcelWriter CreateWriter<TModel>(ExcelProfile<TModel> profile) where TModel : class
            => Configure().WithProfile(profile).BuildWriter();

        /// <summary>
        /// Creates the default test suite containing assembly scanning of <see cref="TestModelProfile"/>
        /// and default test validator registration.
        /// </summary>
        /// <returns>A tuple of configured reader, writer, and service provider.</returns>
        public static (IExcelReader Reader, IExcelWriter Writer, IServiceProvider Provider) CreateDefaultSuite()
        {
            var services = new ServiceCollection();
            services.AddExcelCore(builder =>
            {
                builder.RegisterFromAssemblyContaining<TestModelProfile>();
                builder.UseNonCommercialLicense();
            });
            services.AddScoped<IExcelValidator<TestModel>, TestModelValidator>();

            var provider = services.BuildServiceProvider();
            return (
                provider.GetRequiredService<IExcelReader>(),
                provider.GetRequiredService<IExcelWriter>(),
                provider
            );
        }

        #endregion
    }
}

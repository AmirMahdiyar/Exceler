using Exceler.Abstractions;
using Exceler.DependencyInjection;
using Exceler.Tests.Common.TestDoubles.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using System;

namespace Exceler.Tests.Unit.DependencyInjection
{
    public class LicenseConfigurationTests
    {
        [Fact]
        public void Commercial_license_is_recorded_in_builder_context()
        {
            var services = new ServiceCollection();
            IExcelerBuilder? capturedBuilder = null;

            services.AddExcelCore(builder =>
            {
                capturedBuilder = builder;
                builder.RegisterFromAssemblyContaining<TestModelProfile>();
                builder.UseCommercialLicense();
            });

            capturedBuilder.Should().NotBeNull();
            capturedBuilder!.IsLicenseConfigured.Should().BeTrue();
            capturedBuilder.LicenseContext.Should().Be(LicenseContext.Commercial);
        }

        [Fact]
        public void Non_commercial_license_is_recorded_in_builder_context()
        {
            var services = new ServiceCollection();
            IExcelerBuilder? capturedBuilder = null;

            services.AddExcelCore(builder =>
            {
                capturedBuilder = builder;
                builder.RegisterFromAssemblyContaining<TestModelProfile>();
                builder.UseNonCommercialLicense();
            });

            capturedBuilder.Should().NotBeNull();
            capturedBuilder!.IsLicenseConfigured.Should().BeTrue();
            capturedBuilder.LicenseContext.Should().Be(LicenseContext.NonCommercial);
        }

        [Fact]
        public void Configuring_epplus_engine_without_license_fails()
        {
            var services = new ServiceCollection();

            Action act = () => services.AddExcelCore(builder =>
            {
                builder.UseEPPlusEngine();
                builder.RegisterFromAssemblyContaining<TestModelProfile>();
            });

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*MUST explicitly accept the license terms*");
        }

        [Fact]
        public void Configuring_default_openxml_engine_does_not_require_epplus_license_at_startup()
        {
            var services = new ServiceCollection();

            Action act = () => services.AddExcelCore(builder =>
            {
                builder.RegisterFromAssemblyContaining<TestModelProfile>();
            });

            act.Should().NotThrow();
        }

        [Fact]
        public void Reader_read_without_configured_license_throws_invalid_operation_exception()
        {
            var services = new ServiceCollection();
            services.AddExcelCore(builder =>
            {
                builder.UseOpenXmlEngine();
                builder.RegisterFromAssemblyContaining<TestModelProfile>();
            });

            var provider = services.BuildServiceProvider();
            var reader = provider.GetRequiredService<IExcelReader>();
            using var ms = new MemoryStream();

            var act = () => reader.Read<TestModel, TestModel>(ms).ToList();

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*When reading Excel files, an EPPlus license MUST be explicitly configured*");
        }

        [Fact]
        public async Task Reader_read_in_chunks_without_configured_license_throws_invalid_operation_exception()
        {
            var services = new ServiceCollection();
            services.AddExcelCore(builder =>
            {
                builder.UseOpenXmlEngine();
                builder.RegisterFromAssemblyContaining<TestModelProfile>();
            });

            var provider = services.BuildServiceProvider();
            var reader = provider.GetRequiredService<IExcelReader>();
            using var ms = new MemoryStream();

            var act = async () =>
            {
                await foreach (var chunk in reader.ReadInChunksAsync<TestModel, TestModel>(ms, 10))
                {
                    _ = chunk;
                }
            };

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*When reading Excel files, an EPPlus license MUST be explicitly configured*");
        }

        [Fact]
        public void Reader_read_with_configured_license_passes_license_check()
        {
            var services = new ServiceCollection();
            services.AddExcelCore(builder =>
            {
                builder.UseOpenXmlEngine();
                builder.UseNonCommercialLicense();
                builder.RegisterFromAssemblyContaining<TestModelProfile>();
            });

            var provider = services.BuildServiceProvider();
            var reader = provider.GetRequiredService<IExcelReader>();
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Sheet1");
            ws.Cells[1, 1].Value = "Id";
            ws.Cells[1, 2].Value = "Name";
            using var ms = new MemoryStream();
            package.SaveAs(ms);
            ms.Position = 0;

            var act = () => reader.Read<TestModel, TestModel>(ms).ToList();

            act.Should().NotThrow<InvalidOperationException>();
        }
    }
}

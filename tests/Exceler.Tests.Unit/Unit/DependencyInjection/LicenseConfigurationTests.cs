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
        public void Configuring_excel_core_without_license_fails()
        {
            var services = new ServiceCollection();

            Action act = () => services.AddExcelCore(builder =>
            {
                builder.RegisterFromAssemblyContaining<TestModelProfile>();
            });

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*MUST explicitly accept the license terms*");
        }
    }
}

using Exceler.Abstractions;
using Exceler.DependencyInjection;
using Exceler.Tests.Infrastructure.ModelOfTest;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;

namespace Exceler.Tests.Unit.Infrastructure
{
    public class LicenseConfigurationTests
    {
        [Fact]
        public void WhenConfiguringCommercialLicense_CommercialLicenseContextIsSetAndMarkedConfigured()
        {
            // Arrange
            var services = new ServiceCollection();
            IExcelerBuilder? capturedBuilder = null;

            // Act
            services.AddExcelCore(builder =>
            {
                capturedBuilder = builder;
                builder.RegisterFromAssemblyContaining<TestModelProfile>();
                builder.UseCommercialLicense();
            });

            // Assert
            capturedBuilder.Should().NotBeNull();
            capturedBuilder!.IsLicenseConfigured.Should().BeTrue();
            capturedBuilder.LicenseContext.Should().Be(LicenseContext.Commercial);
        }

        [Fact]
        public void WhenConfiguringNonCommercialLicense_NonCommercialLicenseContextIsSetAndMarkedConfigured()
        {
            // Arrange
            var services = new ServiceCollection();
            IExcelerBuilder? capturedBuilder = null;

            // Act
            services.AddExcelCore(builder =>
            {
                capturedBuilder = builder;
                builder.RegisterFromAssemblyContaining<TestModelProfile>();
                builder.UseNonCommercialLicense();
            });

            // Assert
            capturedBuilder.Should().NotBeNull();
            capturedBuilder!.IsLicenseConfigured.Should().BeTrue();
            capturedBuilder.LicenseContext.Should().Be(LicenseContext.NonCommercial);
        }

        [Fact]
        public void WhenConfiguringExcelCoreWithoutLicense_InvalidOperationExceptionIsThrown()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            Action act = () => services.AddExcelCore(builder =>
            {
                builder.RegisterFromAssemblyContaining<TestModelProfile>();
            });

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*MUST explicitly accept the license terms*");
        }
    }
}

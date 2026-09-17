using Exceler.Abstractions;
using Exceler.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using Xunit;

namespace Exceler.Tests.Unit.Unit.DependencyInjection
{
    public class ExcelerDIBranchesTests
    {
        [Fact]
        public void AddExcelCore_EPPlusEngineWithoutLicense_ThrowsInvalidOperationException()
        {
            var services = new ServiceCollection();

            Action act = () => services.AddExcelCore(builder =>
            {
                builder.UseEPPlusEngine();
                // Intentionally omit UseNonCommercialLicense() or UseCommercialLicense()
            });

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*MUST explicitly accept the license terms*");
        }

        [Fact]
        public void AddExcelCore_EPPlusEngineWithLicense_Succeeds()
        {
            var services = new ServiceCollection();

            services.AddExcelCore(builder =>
            {
                builder.UseEPPlusEngine().UseNonCommercialLicense();
            });

            var provider = services.BuildServiceProvider();
            var writer = provider.GetService<IExcelWriter>();
            writer.Should().NotBeNull();
            writer.Should().BeOfType<Exceler.Core.EPPlus.EPPlusWriterEngine>();
        }

        [Fact]
        public void ExcelerBuilder_NullArguments_ThrowsArgumentNullException()
        {
            Action act1 = () => new ExcelerBuilder(null!);
            act1.Should().Throw<ArgumentNullException>();

            var services = new ServiceCollection();
            var builder = new ExcelerBuilder(services);

            Action act2 = () => builder.RegisterFromAssembly(null!);
            act2.Should().Throw<ArgumentNullException>();
        }
    }
}

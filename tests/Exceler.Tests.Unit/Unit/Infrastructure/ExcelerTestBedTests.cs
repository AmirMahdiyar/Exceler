using Exceler.Abstractions;
using Exceler.Configuration;
using Exceler.Tests.Common.Fixtures;
using Exceler.Tests.Common.TestDoubles.Models;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Exceler.Tests.Unit.Infrastructure
{
    public class ExcelerTestBedTests
    {
        private class SampleModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        private class SampleProfile : ExcelProfile<SampleModel>
        {
            public SampleProfile()
            {
                Map(x => x.Id).ToColumn(1);
                Map(x => x.Name).ToColumn(2);
            }
        }

        private class SampleValidator : IExcelValidator<SampleModel>
        {
            public IEnumerable<string> Validate(SampleModel input)
            {
                if (input.Id < 0) yield return "Invalid ID";
            }
        }

        private class SampleAsyncValidator : IAsyncExcelValidator<SampleModel>
        {
            public async Task<IEnumerable<string>> ValidateAsync(SampleModel input, CancellationToken cancellationToken = default)
            {
                await Task.Yield();
                var errors = new List<string>();
                if (input.Id < 0) errors.Add("Invalid ID");
                return errors;
            }
        }

        private class SampleProcessor : IExcelProcessor<SampleModel, SampleModel>
        {
            public SampleModel Process(SampleModel input)
            {
                input.Name = "Processed_" + input.Name;
                return input;
            }
        }

        private class SampleAsyncProcessor : IAsyncExcelProcessor<SampleModel, SampleModel>
        {
            public async Task<SampleModel> ProcessAsync(SampleModel input, CancellationToken cancellationToken = default)
            {
                await Task.Yield();
                input.Name = "AsyncProcessed_" + input.Name;
                return input;
            }
        }

        [Fact]
        public void TestBed_builder_fluent_configuration_resolves_configured_services()
        {
            var bed = ExcelerTestBed.Configure()
                .WithProfile<SampleModel, SampleProfile>()
                .WithValidator<SampleModel, SampleValidator>()
                .WithAsyncValidator<SampleModel, SampleAsyncValidator>()
                .WithProcessor<SampleModel, SampleModel, SampleProcessor>()
                .WithAsyncProcessor<SampleModel, SampleModel, SampleAsyncProcessor>();

            var reader = bed.BuildReader();
            var writer = bed.BuildWriter();

            reader.Should().NotBeNull();
            writer.Should().NotBeNull();
        }

        [Fact]
        public void TestBed_builder_supports_instance_profile_registration()
        {
            var profileInstance = new SampleProfile();
            var reader = ExcelerTestBed.CreateReader(profileInstance);
            var writer = ExcelerTestBed.CreateWriter(profileInstance);

            reader.Should().NotBeNull();
            writer.Should().NotBeNull();
        }

        [Fact]
        public void TestBed_builder_supports_generic_profile_shortcuts()
        {
            var reader = ExcelerTestBed.CreateReader<SampleModel, SampleProfile>();
            var writer = ExcelerTestBed.CreateWriter<SampleModel, SampleProfile>();

            reader.Should().NotBeNull();
            writer.Should().NotBeNull();
        }

        [Fact]
        public void TestBed_default_suite_provides_fully_wired_dependencies()
        {
            var (reader, writer, provider) = ExcelerTestBed.CreateDefaultSuite();

            reader.Should().NotBeNull();
            writer.Should().NotBeNull();
            provider.Should().NotBeNull();
        }
    }
}

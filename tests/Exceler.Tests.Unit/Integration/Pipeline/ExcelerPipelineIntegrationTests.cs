using Exceler.Tests.Infrastructure.Base;
using Exceler.Tests.Infrastructure.ModelOfTest;
using FluentAssertions;

namespace Exceler.Tests.Integration.Pipeline
{
    public class ExcelerPipelineIntegrationTests : ExcelerTestBase
    {

        [Fact]
        public async Task Written_excel_data_can_be_read_back_without_data_loss_or_corruption()
        {
            // Arrange
            var originalData = new List<TestModel>
        {
            new TestModelBuilder().WithId(1).WithFullName("Vladimir Khorikov").Build(),
            new TestModelBuilder().WithId(2).WithFullName("Clean Architecture").Build()
        };
            using var stream = new MemoryStream();

            // Act
            await Writer.WriteAsync(originalData, stream);
            stream.Position = 0;
            var results = Reader.Read<TestModel, TestModel>(stream).ToList();

            // Assert
            results.Should().HaveCount(2);
            results.All(r => r.IsValid).Should().BeTrue("because the written data inherently matches the profile schema");

            results[0].Data!.Id.Should().Be(1);
            results[0].Data!.FullName.Should().Be("Vladimir Khorikov");

            results[1].Data!.Id.Should().Be(2);
            results[1].Data!.FullName.Should().Be("Clean Architecture");
        }
    }
}

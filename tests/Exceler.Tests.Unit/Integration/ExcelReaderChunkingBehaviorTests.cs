using Exceler.Abstractions;
using Exceler.Tests.Infrastructure.Base;
using Exceler.Tests.Infrastructure.ModelOfTest;
using FluentAssertions;

namespace Exceler.Tests.Integration
{
    public class ExcelReaderChunkingBehaviorTests : ExcelerTestBase
    {

        [Fact]
        public async Task ReadInChunksAsync_processes_large_files_in_configured_chunk_sizes()
        {
            // Arrange
            int totalRecords = 10_000;
            int chunkSize = 2_000;

            var sourceData = TestDataFactory.CreateMany(totalRecords);

            using var stream = new MemoryStream();
            await Writer.WriteAsync(sourceData, stream);
            stream.Position = 0;

            var chunks = new List<List<ExcelRowResult<TestModel>>>();

            // Act
            await foreach (var chunk in Reader.ReadInChunksAsync<TestModel, TestModel>(stream, chunkSize))
            {
                chunks.Add(chunk);
            }

            // Assert
            chunks.Should().HaveCount(5);

            chunks.Should().OnlyContain(chunk => chunk.Count == chunkSize);

            var firstRowData = chunks[0][0].Data;
            firstRowData.Should().NotBeNull();
            firstRowData!.Id.Should().Be(sourceData[0].Id);
            firstRowData.FullName.Should().Be(sourceData[0].FullName);
        }
    }
}

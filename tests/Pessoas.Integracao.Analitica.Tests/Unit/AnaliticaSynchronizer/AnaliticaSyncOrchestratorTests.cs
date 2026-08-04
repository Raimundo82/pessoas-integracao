using FluentAssertions;

using Moq;

using Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer;
using Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer.Synchronizers;
using Pessoas.Integracao.Sync.Application.Contracts;



namespace Pessoas.Integracao.Analitica.Tests.Unit.AnaliticaSynchronizer;

public sealed class AnaliticaSyncOrchestratorTests
{
    [Fact]
    public async Task ShouldInvokeAllRegisteredSynchronizers_WhenExecuting()
    {
        // Arrange
        var synchronizer1 = new Mock<IAnaliticaSynchronizer>();
        var synchronizer2 = new Mock<IAnaliticaSynchronizer>();
        var sut = new AnaliticaSyncOrchestrator([synchronizer1.Object, synchronizer2.Object]);
        var outputs = new List<IZhrOutput> { ZhrOutputTestData.OutputWith() };

        // Act
        await sut.ExecuteAsync(outputs, CancellationToken.None);

        // Assert
        synchronizer1.Verify(h => h.SyncAsync(outputs, It.IsAny<CancellationToken>()), Times.Once);
        synchronizer2.Verify(h => h.SyncAsync(outputs, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldDoNothing_WhenNoHandlersAreRegistered()
    {
        // Arrange
        var sut = new AnaliticaSyncOrchestrator([]);

        // Act
        var act = () => sut.ExecuteAsync([ZhrOutputTestData.OutputWith()], CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }
}

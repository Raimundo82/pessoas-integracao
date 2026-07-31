using FluentAssertions;

using Moq;

using Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer;
using Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer.Synchronizers;
using Pessoas.Integracao.Sync.Application.Contracts;



namespace Pessoas.Integracao.Analitica.Tests.Unit.AnaliticaSynchronizer;

public sealed class AnaliticaSyncOrchestratorTests
{
    [Fact]
    public async Task ShouldInvokeAllRegisteredHandlers_WhenExecuting()
    {
        // Arrange
        var handler1 = new Mock<IAnaliticaSynchronizer>();
        var handler2 = new Mock<IAnaliticaSynchronizer>();
        var sut = new AnaliticaSyncOrchestrator([handler1.Object, handler2.Object]);
        var input = new List<IZhrOutput> { ZhrOutputTestData.OutputWith() };

        // Act
        await sut.ExecuteAsync(input, CancellationToken.None);

        // Assert
        handler1.Verify(h => h.SyncAsync(input, It.IsAny<CancellationToken>()), Times.Once);
        handler2.Verify(h => h.SyncAsync(input, It.IsAny<CancellationToken>()), Times.Once);
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

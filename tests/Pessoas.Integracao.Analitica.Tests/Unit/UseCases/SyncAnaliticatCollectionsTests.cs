using FluentAssertions;

using Moq;

using Pessoas.Integracao.Analitica.Application.UseCases;
using Pessoas.Integracao.Analitica.Infrastructure.Strategies;


namespace Pessoas.Integracao.Analitica.Tests.Unit.UseCases;

public sealed class SyncAnaliticatCollectionsTests
{
    [Fact]
    public async Task ShouldInvokeAllRegisteredHandlers_WhenExecuting()
    {
        // Arrange
        var handler1 = new Mock<ICollectionSyncStrategy>();
        var handler2 = new Mock<ICollectionSyncStrategy>();
        var sut = new SyncAnaliticaCollections([handler1.Object, handler2.Object]);
        var input = ZhrOutputTestData.OutputWith();

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
        var sut = new SyncAnaliticaCollections([]);

        // Act
        var act = () => sut.ExecuteAsync(ZhrOutputTestData.OutputWith(), CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }
}

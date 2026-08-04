using FluentAssertions;

using Moq;

using Pessoas.Integracao.Analitica.Application.Contracts;
using Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer.Synchronizers;
using Pessoas.Integracao.Analitica.Infrastructure.Transformers;
using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Tests.Unit.AnaliticaSynchronizer;

public sealed class AptidaoSynchronizerTests
{
    private readonly Mock<IDataTransformer<ZhrWsAptidaoAptidao, ZhrSAptidao>> _transformer = new();
    private readonly Mock<IAnaliticaRepository<ZhrWsAptidaoAptidao>> _repository = new();

    [Fact]
    public async Task ShouldNotSync_WhenOutputsAreEmpty()
    {
        // Arrange
        var outputs = new List<IZhrOutput>();
        var sut = CreateSut();

        // Act
        await sut.SyncAsync(outputs, CancellationToken.None);

        // Assert
        _transformer.Invocations.Should().BeEmpty();
        _repository.Invocations.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldSyncAptidao_WhenDataIsProvided()
    {
        // Arrange
        string ni = "1", externalId = "3000";
        var timestamp = new DateTimeOffset(new DateTime(2025, 1, 1));
        var item = new ZhrSAptidao { Ni = ni };
        var outputs = new List<IZhrOutput>
        {
            ZhrOutputTestData
            .OutputWith(
                externalId: externalId,
                updateAt: timestamp,
                aptidoes: [item]
                )
        };

        var transformed = new List<ZhrWsAptidaoAptidao>
        {
            new(){ Ni = ni, Numsap = externalId, UpdatedAt = timestamp}
        };

        _transformer.Setup(t => t.Transform(outputs)).Returns(transformed);

        var sut = CreateSut();

        // Act
        await sut.SyncAsync(outputs, CancellationToken.None);

        // Assert
        _transformer.Verify(m => m.Transform(outputs), Times.Once);
        _repository.Verify(r =>
            r.ReplaceMatchingByNiAsync(
                transformed,
                It.IsAny<CancellationToken>()
            ), Times.Once);
    }

    [Fact]
    public async Task ShouldSyncAllTransformedItems_WhenMultipleOutputsAreProvided()
    {
        // Arrange
        string ni1 = "1", externalId1 = "3001";
        string ni2 = "2", externalId2 = "3002";
        var timestamp = new DateTimeOffset(new DateTime(2025, 1, 1, 15, 0, 30));
        var output1 = ZhrOutputTestData.OutputWith(
            ni: ni1,
            externalId: externalId1,
            updateAt: timestamp,
            aptidoes: [new ZhrSAptidao { Ni = ni1, Subty = "0001" }]
        );

        var output2 = ZhrOutputTestData.OutputWith(
            ni: ni2,
            externalId: externalId2,
            updateAt: timestamp,
            aptidoes: [new ZhrSAptidao { Ni = ni1, Subty = "0002" }]
        );

        var outputs = new List<IZhrOutput> { output1, output2 };
        var transformed = new List<ZhrWsAptidaoAptidao>
        {
            new(){ Ni = ni1, Subty = "0001", Numsap = externalId1, UpdatedAt = timestamp },
            new(){ Ni = ni1, Subty = "0002", Numsap = externalId2, UpdatedAt = timestamp }
        };

        _transformer.Setup(m => m.Transform(outputs)).Returns(transformed);

        var sut = CreateSut();

        // Act
        await sut.SyncAsync(outputs, CancellationToken.None);

        // Assert
        _transformer.Verify(m => m.Transform(outputs), Times.Once);
        _repository.Verify(r =>
            r.ReplaceMatchingByNiAsync(
                transformed,
                It.IsAny<CancellationToken>()
            ), Times.Once);
    }

    [Fact]
    public async Task ShouldPropagateCancellationToken_WhenSyncingData()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var outputs = new List<IZhrOutput> { ZhrOutputTestData.OutputWith(aptidoes: [new ZhrSAptidao { Ni = "1" }]) };
        var transformed = new List<ZhrWsAptidaoAptidao> { new() { Ni = "1" } };

        _transformer.Setup(t => t.Transform(outputs)).Returns(transformed);
        var sut = CreateSut();

        // Act
        await sut.SyncAsync(outputs, cancellationToken);

        // Assert
        _repository.Verify(r =>
            r.ReplaceMatchingByNiAsync(
                transformed,
                cancellationToken
            ), Times.Once);
    }

    [Fact]
    public async Task ShouldPropagateException_WhenTransformerThrows()
    {
        // Arrange
        var outputs = new List<IZhrOutput> { ZhrOutputTestData.OutputWith(aptidoes: [new ZhrSAptidao { Ni = "1" }]) };
        _transformer.Setup(t => t.Transform(outputs)).Throws(new InvalidOperationException("Transformer error"));
        var sut = CreateSut();

        // Act & Assert
        Func<Task> act = () => sut.SyncAsync(outputs, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Transformer error");
        _repository.Invocations.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldPropagateException_WhenRepositoryThrows()
    {
        // Arrange
        var outputs = new List<IZhrOutput> { ZhrOutputTestData.OutputWith(aptidoes: [new ZhrSAptidao { Ni = "1" }]) };
        var transformed = new List<ZhrWsAptidaoAptidao> { new() { Ni = "1" } };
        _transformer.Setup(t => t.Transform(outputs)).Returns(transformed);
        _repository.Setup(r => r.ReplaceMatchingByNiAsync(transformed, CancellationToken.None))
                   .Throws(new Exception("Repository error"));
        var sut = CreateSut();

        // Act & Assert
        Func<Task> act = () => sut.SyncAsync(outputs, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("Repository error");
    }

    [Fact]
    public async Task ShouldNotCallRepository_WhenTransformerReturnsEmptyList()
    {
        // Arrange
        var outputs = new List<IZhrOutput> { ZhrOutputTestData.OutputWith(aptidoes: [new ZhrSAptidao { Ni = "1" }]) };
        _transformer.Setup(t => t.Transform(outputs)).Returns([]);
        var sut = CreateSut();

        // Act
        await sut.SyncAsync(outputs, CancellationToken.None);

        // Assert
        _repository.Invocations.Should().BeEmpty();
    }

    private AptidaoSynchronizer CreateSut() => new(_transformer.Object, _repository.Object);

}

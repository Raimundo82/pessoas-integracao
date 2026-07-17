using FluentAssertions;

using Moq;

using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Providers;
using Pessoas.Integracao.Sync.Infrastructure.Providers.FetchResults;

namespace Pessoas.Integracao.Sync.Tests.Unit.Providers;

public class ZhrDataProviderSyncUnitTests
{
    private readonly Mock<IZhrRawDataFetcherStrategy> _strategy1Mock = new();
    private readonly Mock<IZhrRawDataFetcherStrategy> _strategy2Mock = new();

    private ZhrDataProviderSync CreateSut() =>
        new([
            _strategy1Mock.Object,
            _strategy2Mock.Object
        ]);

    private static List<PessoaSyncRef> SomeRefs() =>
    [
        new() { Ni = "21412", ExternalId = "30005902" }
    ];

    private void SetupSuccessfulStrategies()
    {
        _strategy1Mock
            .Setup(s => s.FetchAsync(
                It.IsAny<IReadOnlyList<PessoaSyncRef>>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AptidaoFetchResult([]));

        _strategy2Mock
            .Setup(s => s.FetchAsync(
                It.IsAny<IReadOnlyList<PessoaSyncRef>>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AtribOrgFetchResult([]));
    }

    private void SetupFailingStrategy()
    {
        _strategy1Mock
            .Setup(s => s.FetchAsync(
                It.IsAny<IReadOnlyList<PessoaSyncRef>>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Boom"));

        _strategy2Mock
            .Setup(s => s.FetchAsync(
                It.IsAny<IReadOnlyList<PessoaSyncRef>>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AtribOrgFetchResult([]));
    }

    [Fact]
    public async Task ShouldInvokeAllStrategies()
    {
        // Arrange
        var refs = SomeRefs();

        SetupSuccessfulStrategies();
        var sut = CreateSut();

        // Act
        await sut.SyncZhrDataAsync(refs, null, CancellationToken.None);

        // Assert
        _strategy1Mock.Verify(s => s.FetchAsync(
                refs,
                null,
                It.IsAny<CancellationToken>()),
                Times.Once);

        _strategy2Mock.Verify(s => s.FetchAsync(
                refs,
                null,
                It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Fact]
    public async Task ShouldPassCancellationTokenToAllStrategies()
    {
        // Arrange
        var ct = new CancellationTokenSource().Token;
        SetupSuccessfulStrategies();
        var sut = CreateSut();

        // Act
        await sut.SyncZhrDataAsync(SomeRefs(), null, ct);

        // Assert
        _strategy1Mock.Verify(
            s => s.FetchAsync(
                It.IsAny<IReadOnlyList<PessoaSyncRef>>(),
                It.IsAny<DateOnly?>(),
                ct),
            Times.Once);

        _strategy2Mock.Verify(
            s => s.FetchAsync(
                It.IsAny<IReadOnlyList<PessoaSyncRef>>(),
                It.IsAny<DateOnly?>(),
                ct),
            Times.Once);
    }

    [Fact]
    public async Task ShouldPropagateException_WhenStrategyFails()
    {
        // Arrange
        SetupFailingStrategy();
        var sut = CreateSut();

        // Act
        Func<Task> act = () => sut.SyncZhrDataAsync(
            SomeRefs(),
            null,
            CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Boom");
    }

    [Fact]
    public async Task ShouldComplete_WhenNoStrategiesAreRegistered()
    {
        // Arrange
        var sut = new ZhrDataProviderSync([]);

        // Act
        var act = () => sut.SyncZhrDataAsync(
            SomeRefs(),
            null,
            CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ShouldPassReferenceDateToAllStrategies()
    {
        // Arrange
        var referenceDate = new DateOnly(2025, 01, 15);
        SetupSuccessfulStrategies();
        var sut = CreateSut();

        // Act
        await sut.SyncZhrDataAsync(
            SomeRefs(),
            referenceDate,
            CancellationToken.None);

        // Assert
        _strategy1Mock.Verify(
            s => s.FetchAsync(
                It.IsAny<IReadOnlyList<PessoaSyncRef>>(),
                referenceDate,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _strategy2Mock.Verify(
            s => s.FetchAsync(
                It.IsAny<IReadOnlyList<PessoaSyncRef>>(),
                referenceDate,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

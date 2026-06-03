using FluentAssertions;

using Moq;

using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients.Deltas;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Deltas;

namespace Pessoas.Integracao.Worker.Tests.Unit.Providers;

public sealed class SigdnRhDeltasProviderUnitTests
{
    private readonly Mock<IDeltasClient> _client = new();
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    [Fact]
    public async Task ShouldReturnMappedDeltaKeys_WhenDeltasAreReturnedByProviderClient()
    {
        // Arrange
        _client.Setup(c => c.GetDeltasAsync(It.IsAny<TimePeriod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new ZhrWsGetDeltasPernrOut { Ni = "22600", Pernr = "30002697", Actio = "UPDATE" },
                new ZhrWsGetDeltasPernrOut { Ni = "22700", Pernr = "30002797", Actio = "INSERT" },
                new ZhrWsGetDeltasPernrOut { Ni = "22800", Pernr = "30002897", Actio = "DELETE" },
            ]);

        var provider = new SigdnRhDeltasProvider(_client.Object);
        var period = new TimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

        // Act
        var result = await provider.GetChangedImportKeysAsync(period, _ct);

        // Assert
        result.Should().AllBeOfType<PessoaImportKey>();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo([
            new PessoaImportKey("22600", "30002697"),
            new PessoaImportKey("22700", "30002797"),
            new PessoaImportKey("22800", "30002897"),
        ]);
    }

    [Fact]
    public async Task ShouldReturnSingleDeltaKey_WhenSingleDeltaIsReturnedByProviderClient()
    {
        // Arrange
        _client.Setup(c => c.GetDeltasAsync(It.IsAny<TimePeriod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new ZhrWsGetDeltasPernrOut { Ni = "22600", Pernr = "30002697", Actio = "UPDATE" }
            ]);

        var provider = new SigdnRhDeltasProvider(_client.Object);
        var period = new TimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

        // Act
        var result = await provider.GetChangedImportKeysAsync(period, _ct);

        // Assert
        result.Should().HaveCount(1);
        result.Should().BeEquivalentTo([
            new PessoaImportKey("22600", "30002697")
        ]);
    }

    [Fact]
    public async Task ShouldReturnEmptyDeltaKeys_WhenNoDeltasAreReturnedByProviderClient()
    {
        // Arrange
        _client.Setup(c => c.GetDeltasAsync(It.IsAny<TimePeriod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var provider = new SigdnRhDeltasProvider(_client.Object);
        var period = new TimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

        // Act
        var result = await provider.GetChangedImportKeysAsync(period, _ct);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldThrowException_WhenDeltasProviderClientThrows()
    {
        // Arrange
        _client.Setup(c => c.GetDeltasAsync(It.IsAny<TimePeriod>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SOAP error"));

        var provider = new SigdnRhDeltasProvider(_client.Object);
        var period = new TimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

        // Act
        Func<Task> action = async () =>
            await provider.GetChangedImportKeysAsync(period, _ct);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*SOAP error*");
    }

    [Fact]
    public async Task ShouldForwardCancellationToken_WhenGettingDeltaKeys()
    {
        // Arrange
        using var tokenSource = new CancellationTokenSource();
        CancellationToken? receivedToken = null;

        _client.Setup(c => c.GetDeltasAsync(It.IsAny<TimePeriod>(), It.IsAny<CancellationToken>()))
            .Callback<TimePeriod, CancellationToken>((_, ct) => receivedToken = ct)
            .ReturnsAsync([]);

        var provider = new SigdnRhDeltasProvider(_client.Object);
        var period = new TimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

        // Act
        await provider.GetChangedImportKeysAsync(period, tokenSource.Token);

        // Assert
        receivedToken.Should().Be(tokenSource.Token);
    }
}

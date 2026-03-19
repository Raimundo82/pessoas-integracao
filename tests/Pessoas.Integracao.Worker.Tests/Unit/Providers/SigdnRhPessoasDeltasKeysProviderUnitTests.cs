using FluentAssertions;

using Moq;

using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Contracts;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Deltas;

namespace Pessoas.Integracao.Worker.Tests.Unit.Providers;

public sealed class SigdnRhPessoasDeltasKeysProviderUnitTests
{
    private readonly Mock<IDeltasClient> _client = new();

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

        var provider = new SigdnRhPessoasDeltasKeysProvider(_client.Object);
        var period = new TimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

        // Act
        var result = await provider.GetPessoasDeltasKeysAsync(period, CancellationToken.None);

        // Assert
        result.Should().AllBeOfType<PessoaDeltasKey>();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo([
            new PessoaDeltasKey("22600", "30002697", "UPDATE"),
            new PessoaDeltasKey("22700", "30002797", "INSERT"),
            new PessoaDeltasKey("22800", "30002897", "DELETE"),
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

        var provider = new SigdnRhPessoasDeltasKeysProvider(_client.Object);
        var period = new TimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

        // Act
        var result = await provider.GetPessoasDeltasKeysAsync(period, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.Should().BeEquivalentTo([
            new PessoaDeltasKey("22600", "30002697", "UPDATE")
        ]);
    }

    [Fact]
    public async Task ShouldReturnEmptyDeltaKeys_WhenNoDeltasAreReturnedByProviderClient()
    {
        // Arrange
        _client.Setup(c => c.GetDeltasAsync(It.IsAny<TimePeriod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var provider = new SigdnRhPessoasDeltasKeysProvider(_client.Object);
        var period = new TimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

        // Act
        var result = await provider.GetPessoasDeltasKeysAsync(period, CancellationToken.None);

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

        var provider = new SigdnRhPessoasDeltasKeysProvider(_client.Object);
        var period = new TimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

        // Act
        Func<Task> action = async () =>
            await provider.GetPessoasDeltasKeysAsync(period, CancellationToken.None);

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

        var provider = new SigdnRhPessoasDeltasKeysProvider(_client.Object);
        var period = new TimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

        // Act
        await provider.GetPessoasDeltasKeysAsync(period, tokenSource.Token);

        // Assert
        receivedToken.Should().Be(tokenSource.Token);
    }
}
using FluentAssertions;

using Moq;

using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Contracts;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Tests.Unit.Providers;

public sealed class SigdnRhPessoasImportKeysProviderUnitTests
{
    private readonly Mock<IPersonnelNumbersClient> _client = new();


    [Fact]
    public async Task ShouldReturnMappedImportKeys_WhenPersonnelNumbersAreReturnedByClient()
    {
        // Arrange
        _client.Setup(c => c.GetPersonnelNumbersAsync(It.IsAny<CancellationToken>())).ReturnsAsync([
            new ZhrSListapessoal { Ni = "22600", Numsap = "30002697", Empresa = "3000" },
            new ZhrSListapessoal { Ni = "22700", Numsap = "30002797", Empresa = "3000" },
            new ZhrSListapessoal { Ni = "22800", Numsap = "30002897", Empresa = "3000" },
        ]);
        var provider = new SigdnRhPessoasImportKeysProvider(_client.Object);

        // Act
        var result = await provider.GetSourceImportKeysAsync(CancellationToken.None);

        // Assert 
        result.Should().AllBeOfType<PessoaImportKey>();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(
            [
                new PessoaImportKey("22600", "30002697"),
                new PessoaImportKey("22700", "30002797"),
                new PessoaImportKey("22800", "30002897"),
            ]
        );
    }

    [Fact]
    public async Task ShouldReturnSingleMappedImportKey_WhenSinglePersonnelNumberIsReturnedByClient()
    {
        // Arrange
        _client.Setup(c => c.GetPersonnelNumbersAsync(It.IsAny<CancellationToken>())).ReturnsAsync([
            new ZhrSListapessoal { Ni = "22600", Numsap = "30002697", Empresa = "3000" },
        ]);

        var provider = new SigdnRhPessoasImportKeysProvider(_client.Object);

        // Act
        var result = await provider.GetSourceImportKeysAsync(CancellationToken.None);

        // Assert 
        result.Should().AllBeOfType<PessoaImportKey>();
        result.Should().HaveCount(1);
        result.Should().BeEquivalentTo([new PessoaImportKey("22600", "30002697")]);
    }

    [Fact]
    public async Task ShouldReturnEmptyPessoasImportKeys_WhenNoPersonnelNumbersAreReturnedByClient()
    {
        // Arrange 
        _client.Setup(c => c.GetPersonnelNumbersAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var provider = new SigdnRhPessoasImportKeysProvider(_client.Object);

        // Act
        var result = await provider.GetSourceImportKeysAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldThrowException_WhenPersonnelNumbersClientThrows()
    {
        // Arrange
        _client.Setup(c => c.GetPersonnelNumbersAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("SOAP error"));
        var provider = new SigdnRhPessoasImportKeysProvider(_client.Object);

        // Act
        Func<Task> action = async () => await provider.GetSourceImportKeysAsync(CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*SOAP error*");
    }

    [Fact]
    public async Task ShouldForwardCancellationToken_WhenGettingSourceImportKeys()
    {
        // Arrange
        using var tokenSource = new CancellationTokenSource();
        CancellationToken? receivedToken = null;

        _client.Setup(c => c.GetPersonnelNumbersAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(ct => receivedToken = ct)
            .ReturnsAsync([]);

        var provider = new SigdnRhPessoasImportKeysProvider(_client.Object);

        // Act
        await provider.GetSourceImportKeysAsync(tokenSource.Token);

        // Assert
        receivedToken.Should().Be(tokenSource.Token);
    }
}

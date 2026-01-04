using FluentAssertions;

using Moq;

using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Contracts;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Tests.SigdnRhPessoasProviderTests;

public sealed class SigdnRhPessoasProviderUnitTests : IDisposable
{
    private Mock<IExternalPersonnelNumberClient> _client;

    public SigdnRhPessoasProviderUnitTests()
    {
        _client = new Mock<IExternalPersonnelNumberClient>();
    }

    [Fact]
    public async Task GetPessoasAsync_ReturnsMappedPessoas_FromExternalClient()
    {
        // Arrange
        _client.Setup(c => c.GetExternalPersonnelNumbersAsync(It.IsAny<CancellationToken>())).ReturnsAsync([
            new ZhrSListapessoal { Ni = "22600", Numsap = "30002697", Empresa = "3000" },
            new ZhrSListapessoal { Ni = "22700", Numsap = "30002797", Empresa = "3000" },
            new ZhrSListapessoal { Ni = "22800", Numsap = "30002897", Empresa = "3000" },
        ]);
        var pessoasProvider = new SigdnRhPessoasProvider(_client.Object);

        // Act (When)
        var result = await pessoasProvider.GetPessoasAsync(CancellationToken.None);

        // Assert (Then)
        result.Should().AllBeOfType<Pessoa>();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo([
            new Pessoa { NII = "22600", ExternalId = "30002697" },
            new Pessoa { NII = "22700", ExternalId = "30002797" },
            new Pessoa { NII = "22800", ExternalId = "30002897" }
        ], options => options.ExcludingMissingMembers());
    }


    [Fact]
    public async Task GetPessoasAsync_ReturnsEmptyCollection_WhenExternalClientReturnsEmpty()
    {
        // Arrange 
        _client.Setup(c => c.GetExternalPersonnelNumbersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var pessoasProvider = new SigdnRhPessoasProvider(_client.Object);

        // Act
        var result = await pessoasProvider.GetPessoasAsync(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPessoasAsync_ThrowsException_WhenExternalClientThrows()
    {
        // Arrange
        _client.Setup(c => c.GetExternalPersonnelNumbersAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SOAP error"));
        var pessoasProvider = new SigdnRhPessoasProvider(_client.Object);

        // Act
        Func<Task> action = async () => await pessoasProvider.GetPessoasAsync(CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*SOAP error*");
    }

    [Fact]
    public async Task GetPessoasAsync_CancellationTokenIsPassedToClient()
    {
        // Arrange
        using var tokenSource = new CancellationTokenSource();
        CancellationToken? receivedToken = null;

        _client.Setup(c => c.GetExternalPersonnelNumbersAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(ct => receivedToken = ct)
            .ReturnsAsync([]);

        var pessoasProvider = new SigdnRhPessoasProvider(_client.Object);

        // Act
        await pessoasProvider.GetPessoasAsync(tokenSource.Token);

        // Assert
        receivedToken.Should().Be(tokenSource.Token);
    }

    public void Dispose()
    {
        _client = null!;
        GC.SuppressFinalize(this);
    }
}
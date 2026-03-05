using FluentAssertions;

using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh;

namespace Pessoas.Integracao.Worker.Tests.Unit.Providers;

public sealed class SigdnRhPessoasProviderUnitTests
{

    [Fact]

    public async Task GetPessoasByImportKeysAsync_ReturnsExpectedMappedPessoas()
    {
        // Arrange
        var importKeys = new[]
        {
            new PessoaImportKey("22600", "30002697"),
            new PessoaImportKey("22700", "30002797")
        };

        var expectedPessoas = new[]
        {
            new Pessoa { NII = "22600", ExternalId = "30002697" },
            new Pessoa { NII = "22700", ExternalId = "30002797" }
        };

        var provider = new SigdnRhPessoasProvider();

        // Act
        var pessoas = await provider.GetPessoasByImportKeysAsync(importKeys, default);

        // Assert
        pessoas.Should().NotBeNull();
        pessoas.Should().HaveCount(2);
        pessoas.Should().BeEquivalentTo(expectedPessoas, options => options.ExcludingMissingMembers());
    }

    [Fact]
    public async Task GetPessoasByImportKeysAsync_GivenEmptyImportKeys_ReturnsNoPessoas()
    {
        // Arrange
        var importKeys = Array.Empty<PessoaImportKey>();

        var provider = new SigdnRhPessoasProvider();

        // Act
        var pessoas = await provider.GetPessoasByImportKeysAsync(importKeys, default);

        // Assert
        pessoas.Should().NotBeNull();
        pessoas.Should().BeEmpty();
    }
}
using FluentAssertions;

using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.Enums;
using Pessoas.Integracao.Core.Domain.ValueObjects;

namespace Pessoas.Integracao.Tests.Unit.Domain.Entities;

public sealed class PessoaUpdateFromUnitTests
{
    [Fact]
    public void ShouldUpdateExternalId_WhenSourceHasExternalId()
    {
        var existing = new Pessoa { NII = "123", ExternalId = "old-ext" };
        var source = new Pessoa { NII = "123", ExternalId = "new-ext" };

        existing.UpdateFrom(source);

        existing.ExternalId.Should().Be("new-ext");
    }

    [Fact]
    public void ShouldSetExternalIdToNull_WhenSourceExternalIdIsNull()
    {
        var existing = new Pessoa { NII = "123", ExternalId = "old-ext" };
        var source = new Pessoa { NII = "123", ExternalId = null };

        existing.UpdateFrom(source);

        existing.ExternalId.Should().BeNull();
    }

    [Fact]
    public void ShouldUpdateDadosPessoais_WhenSourceHasDadosPessoais()
    {
        var existing = new Pessoa { NII = "123", DadosPessoais = new DadosPessoais { NomeCompleto = "Old Name" } };
        var source = new Pessoa
        {
            NII = "123",
            DadosPessoais = new DadosPessoais
            {
                NomeCompleto = "New Name",
                Sobrenome = "New Sobrenome",
                Apelidos = "New Apelidos",
                DataNascimento = new DateOnly(1990, 5, 12)
            }
        };

        existing.UpdateFrom(source);

        existing.DadosPessoais.Should().BeEquivalentTo(source.DadosPessoais);
    }

    [Fact]
    public void ShouldUpdateDadosBiometricos_WhenSourceHasDadosBiometricos()
    {
        var existing = new Pessoa { NII = "123", DadosBiometricos = new DadosBiometricos { AlturaEmCm = 170 } };
        var source = new Pessoa
        {
            NII = "123",
            DadosBiometricos = new DadosBiometricos
            {
                AlturaEmCm = 180,
                CorDosOlhos = "castanhos",
                TipoDeSangue = new TipoDeSangue { GrupoSanguineo = GrupoSanguineo.O, Rhesus = Rhesus.POSITIVO }
            }
        };

        existing.UpdateFrom(source);

        existing.DadosBiometricos.Should().BeEquivalentTo(source.DadosBiometricos);
    }

    [Fact]
    public void ShouldNotChangeNII_AfterUpdate()
    {
        var existing = new Pessoa { NII = "123" };
        var source = new Pessoa { NII = "999" };

        existing.UpdateFrom(source);

        existing.NII.Should().Be("123");
    }

    [Fact]
    public void ShouldNotChangeId_AfterUpdate()
    {
        var existing = new Pessoa { NII = "123", Id = 42 };
        var source = new Pessoa { NII = "999", Id = 99 };

        existing.UpdateFrom(source);

        existing.Id.Should().Be(42);
    }

    [Fact]
    public void ShouldNotChangeColocacoes_AfterUpdate()
    {
        var existing = new Pessoa { NII = "123" };
        var originalColocacoes = existing.Colocacoes;
        var source = new Pessoa { NII = "999" };

        existing.UpdateFrom(source);

        existing.Colocacoes.Should().BeSameAs(originalColocacoes);
    }
}

using FluentAssertions;

using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.ValueObjects;

namespace Pessoas.Integracao.Tests.Unit.Domain.Entities;

public sealed class PessoaUpdateColocacoesUnitTests
{
    private static Pessoa NewPessoa(string nii = "123") => new() { NII = nii };

    private static Colocacao NewColocacao(Pessoa pessoa, string externalRef = "U1") =>
        new()
        {
            PessoaId = pessoa.Id,
            Pessoa = pessoa,
            ExternalReference = new UnidadeExternaRef(externalRef),
            Inicio = DateTime.UtcNow
        };

    [Fact]
    public void ShouldBeEmpty_WhenBothExistingAndSourceAreEmpty()
    {
        var existing = NewPessoa();

        existing.UpdateColocacoes([]);

        existing.Colocacoes.Should().BeEmpty();
    }

    [Fact]
    public void ShouldBeEmpty_WhenExistingHasOneItemAndSourceIsEmpty()
    {
        var existing = NewPessoa();
        existing.UpdateColocacoes([NewColocacao(existing)]);

        existing.UpdateColocacoes([]);

        existing.Colocacoes.Should().BeEmpty();
    }

    [Fact]
    public void ShouldBeEmpty_WhenExistingHasMultipleItemsAndSourceIsEmpty()
    {
        var existing = NewPessoa();
        existing.UpdateColocacoes([NewColocacao(existing, "U1"), NewColocacao(existing, "U2"), NewColocacao(existing, "U3")]);

        existing.UpdateColocacoes([]);

        existing.Colocacoes.Should().BeEmpty();
    }

    [Fact]
    public void ShouldHaveOneItem_WhenExistingIsEmptyAndSourceHasOne()
    {
        var existing = NewPessoa();
        var source = NewPessoa("999");
        var incoming = NewColocacao(source, "U1");

        existing.UpdateColocacoes([incoming]);

        existing.Colocacoes.Should().ContainSingle()
            .Which.ExternalReference.Should().Be(new UnidadeExternaRef("U1"));
    }

    [Fact]
    public void ShouldHaveMultipleItems_WhenExistingIsEmptyAndSourceHasMultiple()
    {
        var existing = NewPessoa();
        var source = NewPessoa("999");
        var incoming = new[] { NewColocacao(source, "U1"), NewColocacao(source, "U2"), NewColocacao(source, "U3") };

        existing.UpdateColocacoes(incoming);

        existing.Colocacoes.Should().HaveCount(3);
    }

    [Fact]
    public void ShouldHaveOneItem_WhenExistingHasOneAndSourceHasOne()
    {
        var existing = NewPessoa();
        existing.UpdateColocacoes([NewColocacao(existing, "OLD")]);
        var source = NewPessoa("999");
        var incoming = NewColocacao(source, "NEW");

        existing.UpdateColocacoes([incoming]);

        existing.Colocacoes.Should().ContainSingle()
            .Which.ExternalReference.Should().Be(new UnidadeExternaRef("NEW"));
    }

    [Fact]
    public void ShouldHaveMultipleItems_WhenExistingHasOneAndSourceHasMultiple()
    {
        var existing = NewPessoa();
        existing.UpdateColocacoes([NewColocacao(existing, "OLD")]);
        var source = NewPessoa("999");
        var incoming = new[] { NewColocacao(source, "U1"), NewColocacao(source, "U2") };

        existing.UpdateColocacoes(incoming);

        existing.Colocacoes.Should().HaveCount(2);
    }

    [Fact]
    public void ShouldReplaceAll_WhenExistingHasMultipleAndSourceHasMultiple()
    {
        var existing = NewPessoa();
        existing.UpdateColocacoes([NewColocacao(existing, "OLD1"), NewColocacao(existing, "OLD2")]);
        var source = NewPessoa("999");
        var incoming = new[] { NewColocacao(source, "NEW1"), NewColocacao(source, "NEW2"), NewColocacao(source, "NEW3") };

        existing.UpdateColocacoes(incoming);

        existing.Colocacoes.Should().HaveCount(3)
            .And.NotContain(c => c.ExternalReference.ExternalReference.StartsWith("OLD"));
    }
}

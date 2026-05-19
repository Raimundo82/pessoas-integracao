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
            ExternalReference = new UnidadeExternaRef(externalRef),
            Inicio = DateTime.UtcNow
        };

    [Fact]
    public void ShouldBeEmpty_WhenBothExistingAndSourceAreEmpty()
    {
        // Arrange
        var existing = NewPessoa();

        // Act
        existing.UpdateColocacoes([]);

        // Assert
        existing.Colocacoes.Should().BeEmpty();
    }

    [Fact]
    public void ShouldBeEmpty_WhenExistingHasOneItemAndSourceIsEmpty()
    {
        // Arrange
        var existing = NewPessoa();
        existing.Colocacoes.Add(NewColocacao(existing, "U1"));

        // Act
        existing.UpdateColocacoes([]);

        // Assert
        existing.Colocacoes.Should().BeEmpty();
    }

    [Fact]
    public void ShouldBeEmpty_WhenExistingHasMultipleItemsAndSourceIsEmpty()
    {
        // Arrange
        var existing = NewPessoa();
        existing.Colocacoes.Add(NewColocacao(existing, "U1"));
        existing.Colocacoes.Add(NewColocacao(existing, "U2"));
        existing.Colocacoes.Add(NewColocacao(existing, "U3"));

        // Act
        existing.UpdateColocacoes([]);

        // Assert
        existing.Colocacoes.Should().BeEmpty();
    }

    [Fact]
    public void ShouldHaveOneItem_WhenExistingIsEmptyAndSourceHasOne()
    {
        // Arrange
        var existing = NewPessoa("999");
        existing.Id = 1;

        var source = NewPessoa("999");
        source.Colocacoes.Add(NewColocacao(source, "U1"));

        // Act
        existing.UpdateColocacoes([.. source.Colocacoes]);

        // Assert
        existing.Colocacoes.Should().ContainSingle().Which.ExternalReference.Should().Be(new UnidadeExternaRef("U1"));
        existing.Colocacoes.Should().OnlyContain(c => c.PessoaId == existing.Id);
    }

    [Fact]
    public void ShouldHaveMultipleItems_WhenExistingIsEmptyAndSourceHasMultiple()
    {
        // Arrange
        var existing = NewPessoa("999");
        existing.Id = 1;
        var source = NewPessoa("999");
        source.Colocacoes.Add(NewColocacao(source, "U1"));
        source.Colocacoes.Add(NewColocacao(source, "U2"));
        source.Colocacoes.Add(NewColocacao(source, "U3"));

        // Act
        existing.UpdateColocacoes([.. source.Colocacoes]);

        // Assert
        existing.Colocacoes.Should().HaveCount(3);
        existing.Colocacoes.Should().OnlyContain(c => c.PessoaId == existing.Id);
    }

    [Fact]
    public void ShouldHaveOneItem_WhenExistingHasOneAndSourceHasOne()
    {
        // Arrange
        var existing = NewPessoa("999");
        existing.Id = 1;
        existing.Colocacoes.Add(NewColocacao(existing, "OLD"));

        var source = NewPessoa("999");
        source.Colocacoes.Add(NewColocacao(source, "NEW"));

        // Act
        existing.UpdateColocacoes([.. source.Colocacoes]);

        // Assert
        existing.Colocacoes.Should().ContainSingle().Which.ExternalReference.Should().Be(new UnidadeExternaRef("NEW"));
        existing.Colocacoes.Should().OnlyContain(c => c.PessoaId == existing.Id);
    }

    [Fact]
    public void ShouldHaveMultipleItems_WhenExistingHasOneAndSourceHasMultiple()
    {
        // Arrange
        var existing = NewPessoa("999");
        existing.Id = 1;
        existing.Colocacoes.Add(NewColocacao(existing, "OLD"));
        var source = NewPessoa("999");
        source.Colocacoes.Add(NewColocacao(source, "U1"));
        source.Colocacoes.Add(NewColocacao(source, "U2"));

        // Act
        existing.UpdateColocacoes([.. source.Colocacoes]);

        // Assert
        existing.Colocacoes.Should().HaveCount(2);
        existing.Colocacoes.Should().OnlyContain(c => c.PessoaId == existing.Id);
    }

    [Fact]
    public void ShouldReplaceAll_WhenExistingHasMultipleAndSourceHasMultiple()
    {
        var existing = NewPessoa();
        existing.Id = 1;
        existing.Colocacoes.Add(NewColocacao(existing, "OLD1"));
        existing.Colocacoes.Add(NewColocacao(existing, "OLD2"));
        var source = NewPessoa("999");
        source.Colocacoes.Add(NewColocacao(source, "NEW1"));
        source.Colocacoes.Add(NewColocacao(source, "NEW2"));
        source.Colocacoes.Add(NewColocacao(source, "NEW3"));

        // Act
        existing.UpdateColocacoes([.. source.Colocacoes]);

        // Assert
        existing.Colocacoes.Should().HaveCount(3)
            .And.NotContain(c => c.ExternalReference.ExternalReference.StartsWith("OLD"));
        existing.Colocacoes.Should().OnlyContain(c => c.PessoaId == existing.Id);
    }
}

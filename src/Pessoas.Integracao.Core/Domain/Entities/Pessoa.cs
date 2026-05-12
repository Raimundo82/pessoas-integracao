using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Core.Domain.ValueObjects;
namespace Pessoas.Integracao.Core.Domain.Entities;

[Index(nameof(NII), IsUnique = true)]
public class Pessoa
{
    public int Id { get; set; }
    public required string NII { get; init; }
    public string? ExternalId { get; set; }
    public DadosPessoais DadosPessoais { get; set; } = new DadosPessoais();
    public DadosBiometricos DadosBiometricos { get; set; } = new DadosBiometricos();
    public ICollection<Colocacao> Colocacoes { get; } = [];

    public void UpdateFrom(Pessoa source)
    {
        ExternalId = source.ExternalId;
        DadosPessoais = source.DadosPessoais;
        DadosBiometricos = source.DadosBiometricos;
    }

    public void UpdateColocacoes(IReadOnlyCollection<Colocacao> sourceColocacoes)
    {
        Colocacoes.Clear();
        foreach (var colocacao in sourceColocacoes)
            Colocacoes.Add(colocacao);
    }
}

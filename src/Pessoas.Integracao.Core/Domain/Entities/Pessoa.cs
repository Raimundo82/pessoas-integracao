using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Core.Domain.ValueObjects;
namespace Pessoas.Integracao.Core.Domain.Entities;

[Index(nameof(NII), IsUnique = true)]
public class Pessoa
{
    public int Id { get; set; }
    public required string NII { get; init; }
    public string? ExternalId { get; init; }
    public DadosPessoais DadosPessoais { get; init; } = new DadosPessoais();
    public DadosBiometricos DadosBiometricos { get; init; } = new DadosBiometricos();
}
using Pessoas.Integracao.Core.Domain.ValueObjects;

namespace Pessoas.Integracao.Core.Domain.Entities;

public class Pessoa
{
    public int Id { get; set; }
    public required string NII { get; init; }
    public required DadosPessoais DadosPessoais { get; init; }
    public required DadosBiometricos DadosBiometricos { get; init; }
}
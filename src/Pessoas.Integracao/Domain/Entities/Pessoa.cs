using Pessoas.Integracao.Domain.ValueObjects;

namespace Pessoas.Integracao.Domain.Entities;

public class Pessoa
{
    public int Id { get; set; }
    public required string NII { get; init; }
    public required DadosPessoais DadosPessoais { get; init; }
    public required DadosBiometricos DadosBiometricos { get; init; }
}
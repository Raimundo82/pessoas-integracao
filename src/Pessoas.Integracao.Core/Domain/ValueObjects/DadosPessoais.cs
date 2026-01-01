using Microsoft.EntityFrameworkCore;

namespace Pessoas.Integracao.Core.Domain.ValueObjects;

[Owned]
public class DadosPessoais
{
    public string? NomeCompleto { get; init; }
    public string? Sobrenome { get; init; }
    public string? Apelidos { get; init; }
    public DateTime? DataNascimento { get; init; }
}
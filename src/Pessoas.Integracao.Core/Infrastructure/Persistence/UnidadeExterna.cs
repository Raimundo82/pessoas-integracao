namespace Pessoas.Integracao.Core.Infrastructure.Persistence;

public class UnidadeExterna
{
    public int Id { get; set; }
    public required string ExternalId { get; set; }
    public string? Nome { get; set; }
}

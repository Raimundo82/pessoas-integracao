namespace Pessoas.Integracao.Sync.Infrastructure.Models.Dados;

interface IOutputModel
{
    string Ni { get; set; }
}

public abstract class ZhrSBaseModelOutput : IOutputModel
{
    public required string Ni { get; set; } = null!;
    public DateTime? UpdatedAt { get; set; }
}

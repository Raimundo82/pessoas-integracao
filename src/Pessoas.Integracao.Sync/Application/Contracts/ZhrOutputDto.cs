using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Application.Contracts;

public class ZhrOutputDto
{
    public required string Ni { get; init; }
    public required string ExternalId { get; init; }
    public IReadOnlyList<ZhrSAptidao> Aptidoes { get; init; } = [];
    public IReadOnlyList<ZhrSPessoais> Pessoais { get; init; } = [];
    public IReadOnlyList<ZhrSFamilia> Familias { get; init; } = [];
    public IReadOnlyList<ZhrSOutrosdados> OutrosDados { get; init; } = [];
    public IReadOnlyList<ZhrSDeficiencias> Deficiencias { get; init; } = [];

}

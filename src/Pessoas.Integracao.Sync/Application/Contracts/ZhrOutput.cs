using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Application.Contracts;

public class ZhrOutput
{
    public required string Ni { get; init; }
    public required string ExternalId { get; init; }
    public List<ZhrSAptidao> Aptidoes { get; init; } = [];
    public List<ZhrSPessoais> Pessoais { get; init; } = [];
    public List<ZhrSFamilia> Familias { get; init; } = [];
    public List<ZhrSOutrosdados> OutrosDados { get; init; } = [];
    public List<ZhrSDeficiencias> Deficiencias { get; init; } = [];

}

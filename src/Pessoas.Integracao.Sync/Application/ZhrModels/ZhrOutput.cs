using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Application.ZhrModels;

public class ZhrOutput : IZhrOutput
{
    public required string Ni { get; init; }
    public required string ExternalId { get; init; }
    public DateTimeOffset? UpdateAt { get; init; }
    public IList<ZhrSAptidao>? Aptidoes { get; set; }
    public IList<ZhrSPessoais>? Pessoais { get; set; }
    public IList<ZhrSFamilia>? Familias { get; set; }
    public IList<ZhrSOutrosdados>? OutrosDados { get; set; }
    public IList<ZhrSDeficiencias>? Deficiencias { get; set; }

}

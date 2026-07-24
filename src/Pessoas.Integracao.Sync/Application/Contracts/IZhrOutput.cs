using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Application.Contracts;

public interface IZhrOutput
{
    public string Ni { get; init; }
    public string ExternalId { get; init; }
    public DateTimeOffset? UpdateAt { get; init; }
    public IReadOnlyList<ZhrSAptidao>? Aptidoes { get; set; }
    public IReadOnlyList<ZhrSPessoais>? Pessoais { get; set; }
    public IReadOnlyList<ZhrSFamilia>? Familias { get; set; }
    public IReadOnlyList<ZhrSOutrosdados>? OutrosDados { get; set; }
    public IReadOnlyList<ZhrSDeficiencias>? Deficiencias { get; set; }

}

using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Tests.Unit;

public static class ZhrOutputTestData
{
    public static IZhrOutput OutputWith(
        string ni = "1",
        string externalId = "3000",
        DateTimeOffset? updateAt = null,
        IReadOnlyList<ZhrSAptidao>? aptidoes = null,
        IReadOnlyList<ZhrSPessoais>? pessoais = null,
        IReadOnlyList<ZhrSFamilia>? familias = null,
        IReadOnlyList<ZhrSOutrosdados>? outrosDados = null,
        IReadOnlyList<ZhrSDeficiencias>? deficiencias = null) => new ZhrOutput
        {
            Ni = ni,
            ExternalId = externalId,
            UpdateAt = updateAt,
            Aptidoes = aptidoes,
            Pessoais = pessoais,
            Familias = familias,
            OutrosDados = outrosDados,
            Deficiencias = deficiencias
        };
}

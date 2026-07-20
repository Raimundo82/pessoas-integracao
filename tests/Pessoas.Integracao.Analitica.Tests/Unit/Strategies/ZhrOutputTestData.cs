using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Tests.Unit.Strategies;

public static class ZhrOutputTestData
{
    public static ZhrOutput OutputWith(
        string ni = "1",
        string externalId = "3000",
        IReadOnlyList<ZhrSAptidao>? aptidoes = null,
        IReadOnlyList<ZhrSPessoais>? pessoais = null,
        IReadOnlyList<ZhrSFamilia>? familias = null,
        IReadOnlyList<ZhrSOutrosdados>? outrosDados = null,
        IReadOnlyList<ZhrSDeficiencias>? deficiencias = null) => new()
        {
            Ni = ni,
            ExternalId = externalId,
            Aptidoes = aptidoes,
            Pessoais = pessoais,
            Familias = familias,
            OutrosDados = outrosDados,
            Deficiencias = deficiencias
        };
}

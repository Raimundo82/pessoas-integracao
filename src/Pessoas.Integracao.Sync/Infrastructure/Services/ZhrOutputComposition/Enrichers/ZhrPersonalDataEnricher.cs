using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Contracts;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.ZhrOutputComposition.Enrichers;

public class ZhrPersonalDataEnricher(IZhrFetcherByNi zhrFetcherByNi) : IZhrOutputsEnricher
{

    public async Task<IReadOnlyList<IZhrOutput>> EnrichAsync(
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        IReadOnlyList<IZhrOutput> zhrOutputs,
        CancellationToken ct)
    {
        var pessoaisTask = zhrFetcherByNi.ExecuteAsync<ZhrSPessoais>(pessoaSyncRefs, ct);
        var familiasTask = zhrFetcherByNi.ExecuteAsync<ZhrSFamilia>(pessoaSyncRefs, ct);
        var outrosDadosTask = zhrFetcherByNi.ExecuteAsync<ZhrSOutrosdados>(pessoaSyncRefs, ct);
        var deficienciasTask = zhrFetcherByNi.ExecuteAsync<ZhrSDeficiencias>(pessoaSyncRefs, ct);

        await Task.WhenAll(pessoaisTask, familiasTask, outrosDadosTask, deficienciasTask);

        var pessoaisLookup = (await pessoaisTask).ToLookup(p => p.Ni);
        var familiasLookup = (await familiasTask).ToLookup(f => f.Ni);
        var outrosDadosLookup = (await outrosDadosTask).ToLookup(o => o.Ni);
        var deficienciasLookup = (await deficienciasTask).ToLookup(d => d.Ni);

        foreach (var output in zhrOutputs)
        {
            output.Pessoais = [.. pessoaisLookup[output.Ni]];
            output.Familias = [.. familiasLookup[output.Ni]];
            output.OutrosDados = [.. outrosDadosLookup[output.Ni]];
            output.Deficiencias = [.. deficienciasLookup[output.Ni]];
        }
        return zhrOutputs;
    }
}

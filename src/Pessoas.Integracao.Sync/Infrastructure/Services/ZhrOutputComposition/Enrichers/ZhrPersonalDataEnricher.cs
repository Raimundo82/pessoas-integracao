using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Contracts;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.ZhrOutputComposition.Enrichers;

public class ZhrPersonalDataEnricher(IZhrFetcherByNi zhrFetcherByNi) : IZhrOutputsEnricher
{

    public async Task EnrichAsync(IReadOnlyList<PessoaSyncRef> pessoaSyncRefs, IReadOnlyList<ZhrOutput> zhrOutputs, CancellationToken ct)
    {
        var pessoaisTask = zhrFetcherByNi.ExecuteAsync<ZhrSPessoais>(pessoaSyncRefs, ct);
        var familiasTask = zhrFetcherByNi.ExecuteAsync<ZhrSFamilia>(pessoaSyncRefs, ct);
        var outrosDadosTask = zhrFetcherByNi.ExecuteAsync<ZhrSOutrosdados>(pessoaSyncRefs, ct);
        var deficienciasTask = zhrFetcherByNi.ExecuteAsync<ZhrSDeficiencias>(pessoaSyncRefs, ct);

        await Task.WhenAll(pessoaisTask, familiasTask, outrosDadosTask, deficienciasTask);

        var pessoaisLookup = pessoaisTask.Result.ToLookup(p => p.Ni);
        var familiasLookup = familiasTask.Result.ToLookup(f => f.Ni);
        var outrosDadosLookup = outrosDadosTask.Result.ToLookup(o => o.Ni);
        var deficienciasLookup = deficienciasTask.Result.ToLookup(d => d.Ni);

        foreach (var output in zhrOutputs)
        {
            output.Pessoais.AddRange(pessoaisLookup[output.Ni]);
            output.Familias.AddRange(familiasLookup[output.Ni]);
            output.OutrosDados.AddRange(outrosDadosLookup[output.Ni]);
            output.Deficiencias.AddRange(deficienciasLookup[output.Ni]);
        }
    }
}

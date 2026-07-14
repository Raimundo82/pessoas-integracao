using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Data.ZhrPersistence;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.ZhrOutputComposition.Enrichers;

public class ZhrAptidaoEnricher(ZhrFetcherByNi zhrFetcherByNi) : IZhrOutputsEnricher
{

    public async Task EnrichAsync(
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        IReadOnlyList<ZhrOutput> zhrOutputs,
        CancellationToken ct)
    {
        var aptidoes = await zhrFetcherByNi.ExecuteAsync<ZhrSAptidao>(pessoaSyncRefs, ct);
        var aptidoesLookup = aptidoes.ToLookup(p => p.Ni);

        foreach (var output in zhrOutputs)
        {
            output.Aptidoes.AddRange(aptidoesLookup[output.Ni]);
        }
    }
}

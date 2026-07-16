using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Contracts;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.ZhrOutputComposition.Enrichers;

public class ZhrAptidaoEnricher(IZhrFetcherByNi zhrFetcherByNi) : IZhrOutputsEnricher
{

    public async Task<IReadOnlyList<ZhrOutput>> EnrichAsync(
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        IReadOnlyList<ZhrOutput> zhrOutputs,
        CancellationToken ct)
    {
        var aptidoes = await zhrFetcherByNi.ExecuteAsync<ZhrSAptidao>(pessoaSyncRefs, ct);
        var aptidoesLookup = aptidoes.ToLookup(p => p.Ni);

        foreach (var output in zhrOutputs)
            output.Aptidoes = [.. aptidoesLookup[output.Ni]];

        return zhrOutputs;
    }
}

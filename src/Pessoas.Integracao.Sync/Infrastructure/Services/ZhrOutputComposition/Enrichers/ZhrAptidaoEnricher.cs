using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Contracts;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.ZhrOutputComposition.Enrichers;

public class ZhrAptidaoEnricher(IZhrFetcherByNi zhrFetcherByNi) : IZhrOutputsEnricher
{

    public async Task<IReadOnlyList<IZhrOutput>> EnrichAsync(
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        IReadOnlyList<IZhrOutput> zhrOutputs,
        CancellationToken ct)
    {
        var aptidoes = await zhrFetcherByNi.ExecuteAsync<ZhrSAptidao>(pessoaSyncRefs, ct);
        var aptidoesLookup = aptidoes.ToLookup(p => p.Ni);

        foreach (var output in zhrOutputs)
            output.Aptidoes = [.. aptidoesLookup[output.Ni]];

        return zhrOutputs;
    }
}

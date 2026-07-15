using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Domain.Entities;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.ZhrOutputComposition;

public class ZhrOutputComposer(IEnumerable<IZhrOutputsEnricher> enrichers)
{

    public async Task<IReadOnlyList<ZhrOutput>> ComposeAsync(
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        CancellationToken ct
    )
    {
        IReadOnlyList<ZhrOutput> results = [.. pessoaSyncRefs
            .Select(pRef => new ZhrOutput
            {
                Ni = pRef.Ni,
                ExternalId = pRef.ExternalId,
            })];

        foreach (var enricher in enrichers)
        {
            results = await enricher.EnrichAsync(pessoaSyncRefs, results, ct);
        }

        return results;
    }
}

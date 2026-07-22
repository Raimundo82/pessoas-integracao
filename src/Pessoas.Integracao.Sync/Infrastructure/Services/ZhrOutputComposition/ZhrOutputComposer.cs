using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Services.ZhrOutputComposition.Enrichers;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.ZhrOutputComposition;

public class ZhrOutputComposer(IEnumerable<IZhrOutputsEnricher> enrichers)
{

    public async Task<IReadOnlyList<ZhrOutput>> ComposeAsync(
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        CancellationToken ct
    )
    {
        IReadOnlyList<ZhrOutput> zhrOutputs = [.. pessoaSyncRefs
            .Select(pRef => new ZhrOutput
                {
                    Ni = pRef.Ni,
                    ExternalId = pRef.ExternalId,
                    UpdateAt = pRef.SyncState.UpdatedAt
                }
            )];

        foreach (var enricher in enrichers)
        {
            zhrOutputs = await enricher.EnrichAsync(pessoaSyncRefs, zhrOutputs, ct);
        }

        return zhrOutputs;
    }
}

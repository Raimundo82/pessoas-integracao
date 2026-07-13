
using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Domain.Entities;

namespace Pessoas.Integracao.Sync.Infrastructure;

public class ZhrOutputComposer(IEnumerable<IZhrOutputsEnricher> enrichers)
{

    public async Task<IReadOnlyList<ZhrOutput>> Compose(
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        CancellationToken ct
    )
    {
        var results = pessoaSyncRefs
            .Select(pRef => new ZhrOutput
            {
                Ni = pRef.Ni,
                ExternalId = pRef.ExternalId,
            })
            .ToList();

        foreach (var enricher in enrichers)
        {
            await enricher.EnrichAsync(pessoaSyncRefs, results, ct);
        }

        return results;
    }
}

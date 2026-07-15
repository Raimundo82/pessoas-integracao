using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Contracts;

namespace Pessoas.Integracao.Sync.Infrastructure.Data.ZhrPersistence;

public sealed class ZhrFetcherByNi(ZhrSDbContext dbContext) : IZhrFetcherByNi
{
    private const int BatchSize = 1_000;

    public async Task<IEnumerable<T>> ExecuteAsync<T>(
            IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
            CancellationToken ct
        ) where T : ZhrSBaseModel
    {
        if (pessoaSyncRefs.Count == 0) return [];

        var allNis = pessoaSyncRefs.Select(p => p.Ni).ToList();
        var results = new List<T>();

        foreach (var batch in allNis.Chunk(BatchSize))
        {
            var batchResults = await dbContext.Set<T>()
                .Where(x => batch.Contains(x.Ni))
                .ToListAsync(ct);

            results.AddRange(batchResults);
        }
        return results;
    }
}

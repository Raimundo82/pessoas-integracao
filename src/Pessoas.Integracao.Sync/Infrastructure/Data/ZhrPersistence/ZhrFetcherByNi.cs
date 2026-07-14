using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;

namespace Pessoas.Integracao.Sync.Infrastructure.Data.ZhrPersistence;

public sealed class ZhrFetcherByNi(ZhrSDbContext dbContext)
{

    public async Task<IEnumerable<T>> ExecuteAsync<T>(
            IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
            CancellationToken ct
        ) where T : ZhrSBaseModel
    {
        var nis = pessoaSyncRefs.Select(p => p.Ni);
        return await dbContext.Set<T>()
            .Where(x => nis.Contains(x.Ni))
            .ToListAsync(cancellationToken: ct);
    }
}

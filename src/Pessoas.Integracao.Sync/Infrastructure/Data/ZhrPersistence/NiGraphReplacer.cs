using EFCore.BulkExtensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Infrastructure.Data.ZhrPersistence;

public sealed class NiGraphReplacer(ZhrSDbContext dbContext, ILogger<NiGraphReplacer> logger) : ZhrBasePersistenceReplacer(dbContext, logger)
{
    protected override async Task ExecuteReplaceAsync<T>(
        IReadOnlyList<T> roots,
        IReadOnlyList<ZhrSBaseModel[]> children,
        CancellationToken ct
    )
    {
        var nis = roots.Select(r => r.Ni).ToList();
        await DbContext.Set<T>().Where(e => nis.Contains(e.Ni)).ExecuteDeleteAsync(ct);
        await DbContext.BulkInsertAsync(roots, cancellationToken: ct);
        foreach (var child in children) await DbContext.BulkInsertAsync(child, cancellationToken: ct);
    }
}

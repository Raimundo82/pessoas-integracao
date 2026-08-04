using EFCore.BulkExtensions;

using Microsoft.Extensions.Logging;

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Infrastructure.Data.ZhrPersistence;

public sealed class BulkTableReplacer(ZhrSDbContext dbContext, ILogger<BulkTableReplacer> logger)
    : ZhrBasePersistenceReplacer(dbContext, logger)
{
    protected override async Task ExecuteReplaceAsync<T>(
        IReadOnlyList<T> roots,
        IReadOnlyList<ZhrSBaseModel[]> children,
        CancellationToken ct
    )
    {
        await DbContext.TruncateTableAsync<T>(cascade: true, ct);
        await DbContext.BulkInsertAsync(roots, cancellationToken: ct);
        foreach (var child in children) await DbContext.BulkInsertAsync(child, cancellationToken: ct);
    }
}

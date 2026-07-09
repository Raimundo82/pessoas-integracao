namespace Pessoas.Integracao.Sync.Infrastructure.Data.ZhrPersistence;

using EFCore.BulkExtensions;

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Infrastructure.Data;

public sealed class BulkTableReplacer(ZhrSDbContext dbContext)
{
    public async Task ExecuteAsync<T>(
            IReadOnlyList<T> roots,
            IReadOnlyList<ZhrSBaseModel[]> children,
            CancellationToken ct
        ) where T : ZhrSBaseModelOutput, IOutputModel
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);
        await dbContext.TruncateTableAsync<T>(cascade: true, ct);
        await dbContext.BulkInsertAsync(roots, cancellationToken: ct);
        foreach (var child in children) await dbContext.BulkInsertAsync(child, cancellationToken: ct);
        await transaction.CommitAsync(ct);
    }
}

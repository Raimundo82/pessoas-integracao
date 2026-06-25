namespace Pessoas.Integracao.Sync.Infrastructure.Data.ZhrPersistence;

using EFCore.BulkExtensions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Sync.Infrastructure.Data;
using Pessoas.Integracao.Sync.Infrastructure.Models.Dados;

public sealed class BulkTableReplacer(ZhrSDbContext dbContext)
{
    public async Task ExecuteAsync<T>(
            IReadOnlyList<T> roots,
            IReadOnlyList<ZhrSBaseModel[]> children,
            CancellationToken ct
        ) where T : ZhrSBaseModelOutput, IOutputModel
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);
        await dbContext.Set<T>().ExecuteDeleteAsync(ct);
        foreach (var child in children) await BulkPersistenceHelper.DeleteAllUntypedAsync(dbContext, child, ct);
        await dbContext.BulkInsertAsync(roots, cancellationToken: ct);
        foreach (var child in children) await dbContext.BulkInsertAsync(child, cancellationToken: ct);
        await transaction.CommitAsync(ct);
    }
}

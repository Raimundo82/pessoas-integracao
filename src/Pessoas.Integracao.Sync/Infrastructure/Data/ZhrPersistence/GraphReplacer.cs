namespace Pessoas.Integracao.Sync.Infrastructure.Data.ZhrPersistence;

using EFCore.BulkExtensions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Sync.Infrastructure.Data;
using Pessoas.Integracao.Sync.Infrastructure.Models.Dados;

public sealed class GraphReplacer(ZhrSDbContext dbContext)
{
    private readonly ZhrSDbContext _context = dbContext;
    public async Task ExecuteAsync<T>(
            IReadOnlyList<T> roots,
            IReadOnlyList<ZhrSBaseModel[]> children,
            CancellationToken ct
        ) where T : ZhrSBaseModelOutput, IOutputModel
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        await _context.Set<T>().ExecuteDeleteAsync(ct);
        foreach (var child in children) await BulkPersistenceHelper.DeleteAllUntypedAsync(_context, child, ct);
        await _context.BulkInsertAsync(roots, cancellationToken: ct);
        foreach (var child in children) await _context.BulkInsertAsync(child, cancellationToken: ct);
        await transaction.CommitAsync(ct);
    }
}

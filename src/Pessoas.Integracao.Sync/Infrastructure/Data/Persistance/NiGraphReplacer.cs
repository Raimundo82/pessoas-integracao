namespace Pessoas.Integracao.Sync.Infrastructure.Data.Persistance;

using EFCore.BulkExtensions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Sync.Infrastructure.Data;
using Pessoas.Integracao.Sync.Infrastructure.Models.Dados;

public sealed class NiGraphReplacer(ZhrSDbContext dbContext)
{
    private readonly ZhrSDbContext _context = dbContext;
    public async Task ExecuteAsync<T>(
            IReadOnlyList<T> roots,
            IReadOnlyList<ZhrSBaseModel[]> children,
            CancellationToken ct
        ) where T : ZhrSBaseModelOutput, IOutputModel
    {
        var nis = roots.Select(r => r.Ni).ToList();
        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        await _context.Set<T>().Where(e => nis.Contains(e.Ni)).ExecuteDeleteAsync(ct);
        await _context.BulkInsertAsync(roots, cancellationToken: ct);
        foreach (var child in children) await _context.BulkInsertAsync(child, cancellationToken: ct);
        await transaction.CommitAsync(ct);

    }
}

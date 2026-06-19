namespace Pessoas.Integracao.Sync.Infrastructure.Data.Persistance;

using EFCore.BulkExtensions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Sync.Infrastructure.Data;
using Pessoas.Integracao.Sync.Infrastructure.Models.Dados;

public sealed class UpsertByNi(ZhrSDbContext dbContext)
{
    private readonly ZhrSDbContext _context = dbContext;
    public async Task ExecuteAsync<T>(IReadOnlyList<T> entities, CancellationToken ct) where T : ZhrSBaseModelOutput, IOutputModel
    {
        var nis = entities
          .Select(e => e.Ni)
          .Distinct()
          .ToList();

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        await _context.Set<T>()
            .Where(e => nis.Contains(e.Ni))
            .ExecuteDeleteAsync(ct);

        await _context.BulkInsertAsync(entities, cancellationToken: ct);
        await transaction.CommitAsync(ct);

    }
}

using EFCore.BulkExtensions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Analitica.Application.Contracts;
using Pessoas.Integracao.Analitica.Infrastructure.Data;
using Pessoas.Integracao.Analitica.Models;

namespace Pessoas.Integracao.Analitica.Infrastructure.Repositories;

public class AnaliticaRepository<TEntity>(AnaliticaDbContext context) : IAnaliticaRepository<TEntity>
    where TEntity : ZhrWsBaseModel
{
    private readonly AnaliticaDbContext _context = context;
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public async Task ReplaceMatchingByNiAsync(IReadOnlyList<TEntity> entities, CancellationToken ct)
    {
        var nis = entities
            .Select(e => e.Ni)
            .Distinct()
            .ToList();

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        await _dbSet
            .Where(e => nis.Contains(e.Ni))
            .ExecuteDeleteAsync(ct);

        await _context.BulkInsertAsync(entities, cancellationToken: ct);
        await transaction.CommitAsync(ct);
    }

    public async Task ReplaceAllAsync(IReadOnlyList<TEntity> entities, CancellationToken ct)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        await _dbSet.ExecuteDeleteAsync(ct);
        await _context.BulkInsertAsync(entities, cancellationToken: ct);
        await transaction.CommitAsync(ct);
    }
}

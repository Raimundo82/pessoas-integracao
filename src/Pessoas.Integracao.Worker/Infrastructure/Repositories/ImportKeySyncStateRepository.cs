using EFCore.BulkExtensions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Worker.Application.Contracts;
using Pessoas.Integracao.Worker.Domain.Entities;
using Pessoas.Integracao.Worker.Infrastructure.Data;

namespace Pessoas.Integracao.Worker.Infrastructure.Repositories;

public sealed class ImportKeySyncStateRepository(ImportKeySyncStateDbContext context) : IImportKeySyncStateRepository
{
    private readonly ImportKeySyncStateDbContext _context = context;

    public async Task<IReadOnlyList<ImportKeySyncState>> GetAsync(
        IReadOnlyList<ImportKeySyncState> entities,
        CancellationToken ct)
    {
        var nis = entities
            .Select(e => e.Ni)
            .Distinct()
            .ToList();

        return await _context.ImportKeySyncStates
            .Where(e => nis.Contains(e.Ni))
            .ToListAsync(ct);
    }

    public async Task UpsertAsync(
        IReadOnlyList<ImportKeySyncState> entities,
        CancellationToken ct)
    {
        var list = entities.ToList();

        await _context.BulkInsertOrUpdateAsync(
            list,
            cancellationToken: ct
        );
    }

    public async Task DeleteAsync(
        IReadOnlyList<ImportKeySyncState> entities,
        CancellationToken ct)
    {
        var nis = entities
            .Select(e => e.Ni)
            .Distinct()
            .ToList();

        await _context.ImportKeySyncStates
            .Where(e => nis.Contains(e.Ni))
            .ExecuteDeleteAsync(ct);
    }

    public async Task ReplaceAllAsync(
        IReadOnlyList<ImportKeySyncState> entities,
        CancellationToken ct)
    {
        using var tx = await _context.Database.BeginTransactionAsync(ct);

        await _context.ImportKeySyncStates.ExecuteDeleteAsync(ct);
        await _context.ImportKeySyncStates.AddRangeAsync(entities, ct);

        await _context.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}

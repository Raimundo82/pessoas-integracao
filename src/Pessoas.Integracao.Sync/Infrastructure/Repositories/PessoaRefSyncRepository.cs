using EFCore.BulkExtensions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Data;

namespace Pessoas.Integracao.Sync.Infrastructure.Repositories;

public sealed class PessoaSyncRefRepository(PessoaSyncRefDbContext context) : IPessoaSyncRefRepository
{
    private readonly PessoaSyncRefDbContext _context = context;

    public async Task<IReadOnlyList<PessoaSyncRef>> GetAsync(
        IReadOnlyList<PessoaSyncRef> entities,
        CancellationToken ct)
    {
        var nis = entities
            .Select(e => e.Ni)
            .Distinct()
            .ToList();

        return await _context.PessoaSyncRefs
            .Where(e => nis.Contains(e.Ni))
            .ToListAsync(ct);
    }

    public async Task UpsertAsync(
        IReadOnlyList<PessoaSyncRef> entities,
        CancellationToken ct)
    {
        await _context.BulkInsertOrUpdateAsync(
            entities,
            cancellationToken: ct
        );
    }

    public async Task DeleteAsync(
        IReadOnlyList<PessoaSyncRef> entities,
        CancellationToken ct)
    {
        var nis = entities
            .Select(e => e.Ni)
            .Distinct()
            .ToList();

        await _context.PessoaSyncRefs
            .Where(e => nis.Contains(e.Ni))
            .ExecuteDeleteAsync(ct);
    }

    public async Task ReplaceAllAsync(
        IReadOnlyList<PessoaSyncRef> entities,
        CancellationToken ct)
    {
        using var tx = await _context.Database.BeginTransactionAsync(ct);

        await _context.PessoaSyncRefs.ExecuteDeleteAsync(ct);
        await _context.PessoaSyncRefs.AddRangeAsync(entities, ct);

        await _context.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}

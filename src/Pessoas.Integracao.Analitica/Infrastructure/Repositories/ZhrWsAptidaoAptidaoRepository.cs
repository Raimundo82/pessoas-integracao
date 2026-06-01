using EFCore.BulkExtensions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Analitica.Application.Contracts;
using Pessoas.Integracao.Analitica.Infrastructure.Data;
using Pessoas.Integracao.Analitica.Models;

namespace Pessoas.Integracao.Analitica.Infrastructure.Repositories;

///<summary>
/// Repository responsible for persisting <see cref="ZhrWsAptidaoAptidao"/> records
/// using atomic, transaction‑scoped bulk operations.
/// 
/// This repository supports two operations:
/// 1) <see cref="UpsertByNiiAsync"/> — replaces all exam records for a specific NI.
/// 2) <see cref="ReplaceAllAsync"/> — replaces the entire table with a new dataset.
/// 
/// All operations run inside a database transaction to ensure rollback safety.
/// </summary>
public class ZhrWsAptidaoAptidaoRepository(AnaliticaDbContext context) : IZhrWsAptidaoAptidaoRepository
{
    private readonly AnaliticaDbContext _context = context;

    /// <summary>
    /// Replaces all existing exam records for the NI contained in the provided
    /// <paramref name="entities"/> collection, and inserts the new records.
    /// 
    /// This method performs a per‑NI replace:
    /// - Deletes all rows where NI matches the NI of the first entity.
    /// - Inserts the provided set of rows for that NI.
    /// 
    /// If the NI does not exist in the database, the delete step removes zero rows
    /// and the new records are inserted normally.
    /// 
    /// The entire operation is executed inside a transaction to guarantee atomicity
    /// and rollback safety.
    /// </summary>
    public async Task UpsertByNiiAsync(IReadOnlyList<ZhrWsAptidaoAptidao> entities, CancellationToken ct)
    {
        var nis = entities
            .Select(entity => entity.Ni)
            .Distinct()
            .ToList();

        using var transaction = await _context.Database.BeginTransactionAsync(ct);
        await _context.ZhrWsAptidaoAptidaos
            .Where(e => nis.Contains(e.Ni))
            .ExecuteDeleteAsync(ct);

        await _context.BulkInsertAsync(entities, cancellationToken: ct);
        await transaction.CommitAsync(ct);


    }

    /// <summary>
    /// Replaces the entire <see cref="ZhrWsAptidaoAptidao"/> table with the provided dataset.
    /// 
    /// This method performs a full-table replace:
    /// - Deletes all existing rows.
    /// - Inserts the full provided list of records exactly as received.
    /// 
    /// No deduplication or grouping is performed; the input dataset is treated as
    /// authoritative. The entire operation is executed inside a transaction to
    /// guarantee atomicity and rollback safety.
    /// </summary>
    public async Task ReplaceAllAsync(IReadOnlyList<ZhrWsAptidaoAptidao> entities, CancellationToken ct)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(ct);
        await _context.ZhrWsAptidaoAptidaos.ExecuteDeleteAsync(ct);
        await _context.BulkInsertAsync(entities, cancellationToken: ct);
        await transaction.CommitAsync(ct);

    }
}

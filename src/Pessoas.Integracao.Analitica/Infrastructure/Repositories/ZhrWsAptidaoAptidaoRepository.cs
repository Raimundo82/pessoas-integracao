using EFCore.BulkExtensions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Analitica.Application.Contracts;
using Pessoas.Integracao.Analitica.Infrastructure.Data;
using Pessoas.Integracao.Analitica.Models;

namespace Pessoas.Integracao.Analitica.Infrastructure.Repositories;

public class ZhrWsAptidaoAptidaoRepository(AnaliticaDbContext context) : IZhrWsAptidaoAptidaoRepository
{
    private readonly AnaliticaDbContext _context = context;

    public async Task ReplaceMatchingByNiAsync(IReadOnlyList<ZhrWsAptidaoAptidao> entities, CancellationToken ct)
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

    public async Task ReplaceAllAsync(IReadOnlyList<ZhrWsAptidaoAptidao> entities, CancellationToken ct)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(ct);
        await _context.ZhrWsAptidaoAptidaos.ExecuteDeleteAsync(ct);
        await _context.BulkInsertAsync(entities, cancellationToken: ct);
        await transaction.CommitAsync(ct);

    }
}

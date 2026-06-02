using EFCore.BulkExtensions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Analitica.Application.Contracts;
using Pessoas.Integracao.Analitica.Infrastructure.Data;
using Pessoas.Integracao.Analitica.Models;

namespace Pessoas.Integracao.Analitica.Infrastructure.Repositories;

public class ZhrWsAtribOrgAtribOrgRepository(AnaliticaDbContext context) : IZhrWsAtribOrgAtribOrgRepository
{
    private readonly AnaliticaDbContext _context = context;

    public async Task ReplaceMatchingByNiAsync(IReadOnlyList<ZhrWsAtribOrgAtribOrg> entities, CancellationToken ct)
    {
        var nis = entities
            .Select(entity => entity.Ni)
            .Distinct()
            .ToList();

        using var transaction = await _context.Database.BeginTransactionAsync(ct);
        await _context.ZhrWsAtribOrgAtribOrgs
            .Where(e => nis.Contains(e.Ni))
            .ExecuteDeleteAsync(ct);

        await _context.BulkInsertAsync(entities, cancellationToken: ct);
        await transaction.CommitAsync(ct);
    }

    public async Task ReplaceAllAsync(IReadOnlyList<ZhrWsAtribOrgAtribOrg> entities, CancellationToken ct)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(ct);
        await _context.ZhrWsAtribOrgAtribOrgs.ExecuteDeleteAsync(ct);
        await _context.BulkInsertAsync(entities, cancellationToken: ct);
        await transaction.CommitAsync(ct);
    }
}

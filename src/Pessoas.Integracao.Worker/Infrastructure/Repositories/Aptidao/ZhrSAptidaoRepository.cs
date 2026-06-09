using EFCore.BulkExtensions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Worker.Infrastructure.Data;
using Pessoas.Integracao.Worker.Infrastructure.Models.Dados;

namespace Pessoas.Integracao.Worker.Infrastructure.Repositories.Aptidao;

public class ZhrSAptidaoRepository(ZhrSDbContext context)
{
    private readonly ZhrSDbContext _context = context;

    public async Task ReplaceAllAsync(IReadOnlyList<ZhrSAptidaoOutput> aptidaoOutput, CancellationToken ct)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            await _context.ZhrSAptidoes.ExecuteDeleteAsync(ct);
            await _context.ZhrSAptidaoOutputs.ExecuteDeleteAsync(ct);
            await _context.BulkInsertAsync(aptidaoOutput, new BulkConfig { SetOutputIdentity = true }, cancellationToken: ct);

            var aptidoes = aptidaoOutput
                .SelectMany(o =>
                    {
                        foreach (var child in o.Aptidao)
                            child.ZhrSAptidaoOutputId = o.Id;
                        return o.Aptidao;
                    })
                .ToList();

            await _context.BulkInsertAsync(aptidoes, cancellationToken: ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<IReadOnlyList<ZhrSAptidaoOutput>> GetAllAsync(CancellationToken ct)
    {
        var outputs = await _context.ZhrSAptidaoOutputs.ToListAsync(ct);
        var aptidoes = await _context.ZhrSAptidoes.ToListAsync(ct);
        var lookup = aptidoes.ToLookup(a => a.ZhrSAptidaoOutputId);
        outputs.ForEach(o => o.Aptidao = [.. lookup[o.Id]]);
        return outputs;
    }
}

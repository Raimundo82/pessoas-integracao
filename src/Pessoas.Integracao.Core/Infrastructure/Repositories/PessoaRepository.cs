using EFCore.BulkExtensions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Infrastructure.Data;

namespace Pessoas.Integracao.Core.Infrastructure.Repositories;

public class PessoaRepository(AppDbContext context) : IPessoaRepository
{
    private readonly AppDbContext _context = context;

    public Task AddRangeAsync(IReadOnlyList<Pessoa> pessoas, CancellationToken ct) => _context.AddRangeAsync(pessoas, ct);

    public async Task<Pessoa> AddAsync(Pessoa pessoa, CancellationToken ct) => (await _context.Pessoas.AddAsync(pessoa, ct)).Entity;

    public Task ClearAllAsync(CancellationToken ct) => _context.Pessoas.ExecuteDeleteAsync(ct);

    public async Task<UpsertPessoasResult> UpsertAllAsync(IReadOnlyList<Pessoa> pessoas, CancellationToken ct)
    {

        var niis = pessoas.Select(p => p.NII).ToList();
        var existingPessoas = await _context.Pessoas
            .Where(p => niis.Contains(p.NII))
            .ToDictionaryAsync(p => p.NII, ct);

        int added = 0;
        int updated = 0;

        foreach (var pessoa in pessoas)
        {
            if (existingPessoas.TryGetValue(pessoa.NII, out var existingPessoa))
            {
                existingPessoa.UpdateFrom(pessoa);
                updated++;
            }
            else
            {
                _context.Pessoas.Add(pessoa);
                added++;
            }
        }

        return new UpsertPessoasResult(added, updated);
    }

    public async Task<IReadOnlyList<Pessoa>> GetAllAsync(CancellationToken ct) =>
        await _context.Pessoas.ToListAsync(ct);

    public async Task<IReadOnlyList<Pessoa>> GetPessoasByNiiAsync(IReadOnlyList<string> niis, CancellationToken ct)
    {
        return await _context.Pessoas.Where(p => niis.Contains(p.NII)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PessoaImportKey>> GetExistingImportKeysAsync(CancellationToken cancellationToken)
    {
        return await _context.Pessoas
            .Select(p => new PessoaImportKey(p.NII, p.ExternalId))
            .ToListAsync(cancellationToken);
    }

    public async Task ReplaceAllAsync(IReadOnlyList<Pessoa> pessoas, CancellationToken ct)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            await ClearAllAsync(ct);
            await _context.BulkInsertAsync(pessoas, cancellationToken: ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}

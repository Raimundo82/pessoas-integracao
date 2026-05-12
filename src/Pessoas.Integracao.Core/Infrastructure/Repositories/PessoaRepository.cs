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

    public async Task UpsertAllAsync(IReadOnlyList<Pessoa> pessoas, CancellationToken ct)
    {
        var dedupedPessoas = pessoas.DistinctBy(p => p.NII).ToList();
        var niis = dedupedPessoas.Select(p => p.NII).ToList();
        var existingPessoas = await _context.Pessoas
            .Include(p => p.Colocacoes)
            .Where(p => niis.Contains(p.NII))
            .ToDictionaryAsync(p => p.NII, ct);

        foreach (var pessoa in dedupedPessoas)
        {
            if (existingPessoas.TryGetValue(pessoa.NII, out var trackedPessoa))
            {
                trackedPessoa.UpdateFrom(pessoa);
            }
            else
            {
                _context.Pessoas.Add(pessoa);
            }
        }
        await _context.SaveChangesAsync(cancellationToken: ct);
    }

    public async Task<IReadOnlyList<Pessoa>> GetAllAsync(CancellationToken ct) =>
        await _context.Pessoas.ToListAsync(ct);

    public async Task<IReadOnlyList<Pessoa>> GetPessoasByNiiAsync(IReadOnlyList<string> niis, CancellationToken ct)
    {
        var distinctNiis = niis.Distinct().ToList();
        return await _context.Pessoas
            .AsNoTracking()
            .Where(p => distinctNiis.Contains(p.NII))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PessoaImportKey>> GetExistingImportKeysAsync(CancellationToken cancellationToken)
    {
        return await _context.Pessoas
            .Select(p => new PessoaImportKey(p.NII, p.ExternalId))
            .ToListAsync(cancellationToken);
    }

    public async Task ReplaceAllAsync(IReadOnlyList<Pessoa> pessoas, CancellationToken ct)
    {
        var dedupedPessoas = pessoas.DistinctBy(p => p.NII).ToList();

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        await _context.Colocacoes.ExecuteDeleteAsync(ct);
        await _context.Pessoas.ExecuteDeleteAsync(ct);

        await _context.BulkInsertAsync(dedupedPessoas, cancellationToken: ct);

        var existingPessoas = await _context.Pessoas.ToDictionaryAsync(p => p.NII, ct);

        var colocacoes = dedupedPessoas
            .SelectMany(p =>
            {
                var pessoaId = existingPessoas[p.NII].Id;

                foreach (var colocacao in p.Colocacoes)
                {
                    colocacao.PessoaId = pessoaId;
                }

                return p.Colocacoes;
            })
            .ToList();

        if (colocacoes.Count > 0)
        {
            await _context.BulkInsertAsync(colocacoes, cancellationToken: ct);
        }

        await transaction.CommitAsync(ct);
    }
}

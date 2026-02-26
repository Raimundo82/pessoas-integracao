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

    public async Task AddOrUpdateAllAsync(IReadOnlyList<Pessoa> pessoas, CancellationToken ct)
    {

        var niis = pessoas.Select(p => p.NII).ToList();
        var existingPessoas = await _context.Pessoas
            .Where(p => niis.Contains(p.NII))
            .ToDictionaryAsync(p => p.NII, ct);

        foreach (var pessoa in pessoas)
        {
            if (existingPessoas.TryGetValue(pessoa.NII, out var existingPessoa))
            {
                existingPessoa.UpdateFrom(pessoa);
            }
            else
            {
                _context.Pessoas.Add(pessoa);
            }
        }
    }

    public async Task<IReadOnlyList<Pessoa>> GetAllAsync(CancellationToken ct) =>
        await _context.Pessoas.ToListAsync(ct);

    public async Task<IReadOnlyList<PessoaImportKey>> GetExistingImportKeysAsync(CancellationToken cancellationToken)
    {
        return await _context.Pessoas
            .Select(p => new PessoaImportKey(p.NII, p.ExternalId))
            .ToListAsync(cancellationToken);
    }
}
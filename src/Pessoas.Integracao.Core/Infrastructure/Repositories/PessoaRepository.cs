using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.Interfaces;
using Pessoas.Integracao.Core.Infrastructure.Data;

namespace Pessoas.Integracao.Core.Infrastructure.Repositories;

public class PessoaRepository(AppDbContext context) : IPessoaRepository
{
    private readonly AppDbContext _context = context;

    public Task AddRangeAsync(IReadOnlyCollection<Pessoa> pessoas, CancellationToken ct) => _context.AddRangeAsync(pessoas, ct);

    public async Task<Pessoa> AddAsync(Pessoa pessoa, CancellationToken ct) => (await _context.Pessoas.AddAsync(pessoa, ct)).Entity;

    public Task ClearAllAsync(CancellationToken ct) => _context.Pessoas.ExecuteDeleteAsync(ct);

    public async Task<IReadOnlyCollection<Pessoa>> GetAllAsync(CancellationToken ct) =>
        (await _context.Pessoas.ToListAsync(ct)).AsReadOnly();
}
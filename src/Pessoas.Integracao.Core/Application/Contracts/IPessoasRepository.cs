using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Core.Application.Contracts;

public interface IPessoaRepository
{
    Task<Pessoa> AddAsync(Pessoa pessoa, CancellationToken ct);
    Task AddRangeAsync(IReadOnlyList<Pessoa> pessoas, CancellationToken ct);
    Task ClearAllAsync(CancellationToken ct);
    Task AddOrUpdateAllAsync(IReadOnlyList<Pessoa> pessoas, CancellationToken ct);
    Task<IReadOnlyList<Pessoa>> GetAllAsync(CancellationToken ct);
}
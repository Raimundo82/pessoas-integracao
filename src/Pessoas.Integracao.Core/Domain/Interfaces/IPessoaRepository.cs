using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Core.Domain.Interfaces;

public interface IPessoaRepository
{
    Task<Pessoa> AddAsync(Pessoa pessoa, CancellationToken ct);
    Task AddRangeAsync(IReadOnlyCollection<Pessoa> pessoas, CancellationToken ct);
    Task ClearAllAsync(CancellationToken ct);
    Task<IReadOnlyCollection<Pessoa>> GetAllAsync(CancellationToken ct);
}
using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Core.Domain.Interfaces;

public interface IPessoaRepository
{
    Task AddAsync(Pessoa pessoa, CancellationToken ct);
    Task AddRangeAsync(IReadOnlyCollection<Pessoa> pessoas, CancellationToken ct);
    Task ClearAllAsync(CancellationToken ct);
}
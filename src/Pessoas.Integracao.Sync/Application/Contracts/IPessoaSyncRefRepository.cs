using Pessoas.Integracao.Sync.Domain.Entities;

namespace Pessoas.Integracao.Sync.Application.Contracts;

public interface IPessoaSyncRefRepository
{
    Task<IReadOnlyList<PessoaSyncRef>> GetAsync(
        IReadOnlyList<PessoaSyncRef> entities,
        CancellationToken ct);

    Task UpsertAsync(
        IReadOnlyList<PessoaSyncRef> entities,
        CancellationToken ct);

    Task DeleteAsync(
        IReadOnlyList<PessoaSyncRef> entities,
        CancellationToken ct);

    Task ReplaceAllAsync(
        IReadOnlyList<PessoaSyncRef> entities,
        CancellationToken ct);
}



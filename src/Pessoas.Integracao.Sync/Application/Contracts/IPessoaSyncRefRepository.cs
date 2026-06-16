using Pessoas.Integracao.Sync.Domain.Entities;

namespace Pessoas.Integracao.Sync.Application.Contracts;

public interface IPessoaSyncRefRepository
{
    Task<IReadOnlyList<PessoaSyncRef>> GetByNiAsync(
        IReadOnlyList<string> niList,
        CancellationToken ct);

    Task UpsertAsync(
        IReadOnlyList<PessoaSyncRef> entities,
        CancellationToken ct);

    Task DeleteByNiAsync(
        IReadOnlyList<string> niList,
        CancellationToken ct);

    Task ReplaceAllAsync(
        IReadOnlyList<PessoaSyncRef> entities,
        CancellationToken ct);
}



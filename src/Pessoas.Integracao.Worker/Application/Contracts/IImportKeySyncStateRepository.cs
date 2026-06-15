using Pessoas.Integracao.Worker.Domain.Entities;

namespace Pessoas.Integracao.Worker.Application.Contracts;

public interface IImportKeySyncStateRepository
{
    Task<IReadOnlyList<ImportKeySyncState>> GetAsync(
        IReadOnlyList<ImportKeySyncState> entities,
        CancellationToken ct);

    Task UpsertAsync(
        IReadOnlyList<ImportKeySyncState> entities,
        CancellationToken ct);

    Task DeleteAsync(
        IReadOnlyList<ImportKeySyncState> entities,
        CancellationToken ct);

    Task ReplaceAllAsync(
        IReadOnlyList<ImportKeySyncState> entities,
        CancellationToken ct);
}



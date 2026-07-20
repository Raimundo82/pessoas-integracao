using Pessoas.Integracao.Sync.Application.Contracts;

namespace Pessoas.Integracao.Analitica.Infrastructure.Strategies;

public interface ICollectionSyncStrategy
{
    Task SyncAsync(ZhrOutput input, CancellationToken ct);
}

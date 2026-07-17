using Pessoas.Integracao.Sync.Application.Contracts;

namespace Pessoas.Integracao.Analitica.Infrastructure.SyncHandlers;

public interface IZhrCollectionSyncHandler
{
    Task SyncAsync(ZhrOutput input, CancellationToken ct);
}

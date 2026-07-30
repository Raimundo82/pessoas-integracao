using Pessoas.Integracao.Sync.Application.Contracts;

namespace Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer.Synchronizers;

public interface IAnaliticaSynchronizer
{
    Task SyncAsync(IZhrOutput input, CancellationToken ct);
}

using Pessoas.Integracao.Sync.Application.Contracts;

namespace Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer.Synchronizers;

public interface IAnaliticaSynchronizer
{
    Task SyncAsync(IReadOnlyList<IZhrOutput> inputs, CancellationToken ct);
}

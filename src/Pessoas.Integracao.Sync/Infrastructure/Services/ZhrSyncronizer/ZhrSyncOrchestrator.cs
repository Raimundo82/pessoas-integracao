using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Services.ZhrSyncronizer.Syncronizers;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.ZhrSyncronizer;

public class ZhrSyncOrchestrator(IEnumerable<IZhrSyncronizer> zhrRawDataFetcherStrategies) : IZhrSyncOrchestrator
{
    private readonly IEnumerable<IZhrSyncronizer> _zhrRawDataFetcherStrategies = zhrRawDataFetcherStrategies;
    public async Task SyncZhrDataAsync(
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        DateOnly? referenceDate,
        CancellationToken ct)
    {
        await Task.WhenAll(
            _zhrRawDataFetcherStrategies.Select(async strategy =>
            {
                await strategy.FetchAsync(
                    pessoaSyncRefs,
                    referenceDate,
                    ct);
            }));
    }
}

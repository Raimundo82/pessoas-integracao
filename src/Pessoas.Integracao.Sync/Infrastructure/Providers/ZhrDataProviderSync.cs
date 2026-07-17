using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Domain.Entities;

namespace Pessoas.Integracao.Sync.Infrastructure.Providers;

public class ZhrDataProviderSync(IEnumerable<IZhrRawDataFetcherStrategy> zhrRawDataFetcherStrategies) : IZhrDataProviderSync
{
    private readonly IEnumerable<IZhrRawDataFetcherStrategy> _zhrRawDataFetcherStrategies = zhrRawDataFetcherStrategies;
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

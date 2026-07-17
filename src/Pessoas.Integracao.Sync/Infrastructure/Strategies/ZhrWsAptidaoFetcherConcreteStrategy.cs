using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Clients;
using Pessoas.Integracao.Sync.Infrastructure.Providers.FetchResults;

namespace Pessoas.Integracao.Sync.Infrastructure.Strategies;

/// <summary>
/// Shared aggregation object.
/// Each strategy owns exactly one property and may only replace
/// the property value. Properties must never be mutated.
/// </summary>
public class ZhrWsAptidaoFetcherConcreteStrategy(IZhrWsGenericClient client) : ZhrRawDataFetcherStrategyBase(client), IZhrRawDataFetcherStrategy

{
    public async Task<IZhrFetchResult> FetchAsync(
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        DateOnly? referenceDate,
        CancellationToken ct)
    {
        var aptidaoOutputs = await ExecuteAsync(
            static (c, inputs) => c.ZhrWsAptidaoAsync(new ZhrWsAptidao { Input = inputs }),
            r => r?.ZhrWsAptidaoResponse?.Output,
            pessoaSyncRefs,
            referenceDate,
            ct);

        return new AptidaoFetchResult(aptidaoOutputs);
    }
}

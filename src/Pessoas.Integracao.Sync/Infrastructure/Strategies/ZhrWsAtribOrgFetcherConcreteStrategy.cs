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
public sealed class ZhrWsAtribOrgFetcherConcreteStrategy(IZhrWsGenericClient client)
    : ZhrRawDataFetcherStrategyBase(client),
      IZhrRawDataFetcherStrategy
{
    public async Task<IZhrFetchResult> FetchAsync(
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        DateOnly? referenceDate,
        CancellationToken ct)
    {
        var atribOrgOutputs = await ExecuteAsync(
            static (c, inputs) =>
                c.ZhrWsAtribOrgAsync(new ZhrWsAtribOrg { Input = inputs }),
            r => r?.ZhrWsAtribOrgResponse?.Output,
            pessoaSyncRefs,
            referenceDate,
            ct);

        return new AtribOrgFetchResult(atribOrgOutputs);
    }
}

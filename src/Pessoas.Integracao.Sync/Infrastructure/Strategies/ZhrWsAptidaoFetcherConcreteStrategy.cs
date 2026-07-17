using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Clients;

namespace Pessoas.Integracao.Sync.Infrastructure.Strategies;

/// <summary>
/// Shared aggregation object.
/// Each strategy owns exactly one property and may only replace
/// the property value. Properties must never be mutated.
/// </summary>
public class ZhrWsAptidaoFetcherConcreteStrategy(IZhrWsGenericClient client) : IZhrRawDataFetcherStrategy

{
    public async Task<ZhrSBaseModelOutput[]?> FetchAsync(
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        DateOnly? referenceDate = null,
        CancellationToken ct = default)
    {
        var aptidaoOutputs = await client.CallAsync(
            static (c, inputs) => c.ZhrWsAptidaoAsync(new ZhrWsAptidao { Input = inputs }),
            r => r?.ZhrWsAptidaoResponse?.Output,
            pessoaSyncRefs,
            referenceDate,
            ct);

        return aptidaoOutputs ?? [];
    }
}

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Clients;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.ZhrDataProvider.Providers;

/// <summary>
/// Shared aggregation object.
/// Each strategy owns exactly one property and may only replace
/// the property value. Properties must never be mutated.
/// </summary>
public sealed class ZhrAtribOrgProvider(IZhrWsGenericClient client) : IZhrRawDataFetcherStrategy
{
    public async Task<ZhrSBaseModelOutput[]?> FetchAsync(
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        DateOnly? referenceDate = null,
        CancellationToken ct = default)
    {
        var atribOrgOutputs = await client.CallAsync(
            static (c, inputs) =>
                c.ZhrWsAtribOrgAsync(new ZhrWsAtribOrg { Input = inputs }),
            r => r?.ZhrWsAtribOrgResponse?.Output,
            pessoaSyncRefs,
            referenceDate,
            ct);

        return atribOrgOutputs ?? [];
    }
}

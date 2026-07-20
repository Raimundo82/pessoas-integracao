using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Clients;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.ZhrSyncronizer.Syncronizers;

public sealed class ZhrAptidaoProvider(IZhrWsGenericClient client) : IZhrSyncronizer

{
    public async Task<ZhrSBaseModelOutput[]?> FetchAsync(
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        DateOnly? referenceDate = null,
        CancellationToken ct = default)
    {
        var aptidaoOutputs = await client.CallAsync(
            static (c, inputs) => c.ZhrWsAptidaoAsync(new ZhrWsAptidao { Input = inputs }),
            static r => r?.ZhrWsAptidaoResponse?.Output,
            pessoaSyncRefs,
            referenceDate,
            ct);

        return aptidaoOutputs ?? [];
    }
}

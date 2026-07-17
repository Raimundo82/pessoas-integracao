using Microsoft.Extensions.Options;

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Configuration;
using Pessoas.Integracao.Sync.Infrastructure.Factories;
using Pessoas.Integracao.Sync.Infrastructure.Services.ReferenceDate;

namespace Pessoas.Integracao.Sync.Infrastructure.Clients;

public class ZhrWsGenericClient(
    IZhrWsGenericClientFactory<zhr_wsClient, zhr_ws> clientFactory,
    IOptions<ZhrWsSettings> settings,
    IZhrReferenceDateFormatter referenceDateFormatter
) : IZhrWsGenericClient
{
    private readonly IZhrWsGenericClientFactory<zhr_wsClient, zhr_ws> _clientFactory = clientFactory;
    private readonly ZhrWsSettings _settings = settings.Value;
    private readonly IZhrReferenceDateFormatter _referenceDateFormatter = referenceDateFormatter;

    public async Task<ZhrSBaseModelOutput[]?> CallAsync<TResponse>(
        Func<zhr_wsClient, ZhrWsInputStruct[], Task<TResponse?>> zhrSOperation,
        Func<TResponse, ZhrSBaseModelOutput[]?> selectOutput,
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        DateOnly? referenceDate = null,
        CancellationToken ct = default
    ) where TResponse : class, IZhrWsBaseResponse
    {
        if (pessoaSyncRefs == null || pessoaSyncRefs.Count == 0) return [];

        using var client = _clientFactory.CreateClient();
        using var registration = ct.Register(() => client.Abort());

        var inputs = pessoaSyncRefs.Select(pessoaRef =>
            new ZhrWsInputStruct
            {
                Ni = pessoaRef.Ni,
                Numsap = pessoaRef.ExternalId,
                Empresa = _settings.Empresa,
                Dtreferencia = referenceDate.HasValue ? _referenceDateFormatter.Format(referenceDate.Value) : string.Empty
            }).ToArray();

        var response = await zhrSOperation(client, inputs);
        return response == null ? null : selectOutput(response);
    }
}

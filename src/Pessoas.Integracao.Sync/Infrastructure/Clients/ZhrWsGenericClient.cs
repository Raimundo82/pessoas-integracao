using Microsoft.Extensions.Options;

using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Configuration;
using Pessoas.Integracao.Sync.Infrastructure.Factories;
using Pessoas.Integracao.Sync.Infrastructure.Models.Dados;
using Pessoas.Integracao.Sync.Infrastructure.Services.ReferenceDate;

namespace Pessoas.Integracao.Sync.Infrastructure.Clients;

public class ZhrWsGenericClient(
    IZhrWsGenericClientFactory<ZHR_WSClient, ZHR_WS> clientFactory,
    IOptions<ZhrWsSettings> settings,
    IZhrReferenceDateFormatter referenceDateFormatter
) : IZhrWsGenericClient
{
    private readonly IZhrWsGenericClientFactory<ZHR_WSClient, ZHR_WS> _clientFactory = clientFactory;
    private readonly ZhrWsSettings _settings = settings.Value;
    private readonly IZhrReferenceDateFormatter _referenceDateFormatter = referenceDateFormatter;

    public async Task<TResponse> CallAsync<TResponse>(
        Func<ZHR_WSClient, ZhrWsInputStruct[], Task<TResponse>> zhrWsOperation,
        IReadOnlyCollection<PessoaSyncRef> pessoaSyncRefs,
        DateOnly? referenceDate = null,
        CancellationToken ct = default
    ) where TResponse : IZhrWsBaseResponse
    {
        var client = _clientFactory.CreateClient();
        var inputs = pessoaSyncRefs.Select(pessoaRef =>
            new ZhrWsInputStruct
            {
                Ni = pessoaRef.Ni,
                Numsap = pessoaRef.ExternalId,
                Empresa = _settings.Empresa,
                Dtreferencia = referenceDate.HasValue ? _referenceDateFormatter.Format(referenceDate.Value) : string.Empty
            }).ToArray();

        return await zhrWsOperation(client, inputs).WaitAsync(ct);
    }
}

using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Models.Dados;

namespace Pessoas.Integracao.Sync.Infrastructure.Clients;

public interface IZhrWsGenericClient
{
    Task<TResponse> CallAsync<TResponse>(
        Func<ZHR_WSClient, ZhrWsInputStruct[], Task<TResponse>> zhrSOperation,
        IReadOnlyCollection<PessoaSyncRef> pessoaSyncRefs,
        CancellationToken ct
    ) where TResponse : IZhrWsBaseResponse;
}

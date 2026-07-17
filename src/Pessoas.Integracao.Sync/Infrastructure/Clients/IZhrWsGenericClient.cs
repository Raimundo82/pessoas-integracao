using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;


namespace Pessoas.Integracao.Sync.Infrastructure.Clients;

public interface IZhrWsGenericClient
{
    Task<ZhrSBaseModelOutput[]?> CallAsync<TResponse>(
        Func<zhr_wsClient, ZhrWsInputStruct[], Task<TResponse?>> zhrSOperation,
        Func<TResponse?, ZhrSBaseModelOutput[]?> selectOutput,
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        DateOnly? referenceDate = null,
        CancellationToken ct = default
    ) where TResponse : IZhrWsBaseResponse;
}

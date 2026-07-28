using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;


namespace Pessoas.Integracao.Sync.Infrastructure.Clients;

public interface IZhrWsGenericClient
{
    Task<TResponse?> CallAsync<TResponse1, TResponse>(
        Func<zhr_wsClient, ZhrWsInputStruct[], Task<TResponse1?>> zhrSOperation,
        Func<TResponse1, TResponse?> responseSelector,
        IReadOnlyCollection<PessoaSyncRef> pessoaSyncRefs,
        DateOnly? referenceDate = null,
        CancellationToken ct = default
    )
    where TResponse1 : IZhrWsBaseResponse1
    where TResponse : IZhrWsBaseResponse;
}

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Clients;

namespace Pessoas.Integracao.Sync.Infrastructure.Strategies;

/// <summary>
/// Strategies must only modify their own assigned property in ZhrRawData; 
/// any shared state mutation will cause race conditions
/// during concurrent execution
/// </summary>
public abstract class ZhrRawDataFetcherStrategyBase(IZhrWsGenericClient client)
{
    protected async Task<TOutput[]> ExecuteAsync<TResponse, TOutput>(
        Func<zhr_wsClient, ZhrWsInputStruct[], Task<TResponse?>> call,
        Func<TResponse?, TOutput[]?> selectOutput,
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        DateOnly? referenceDate,
        CancellationToken ct)
        where TResponse : IZhrWsBaseResponse
    {
        var response = await client.CallAsync(
            call,
            pessoaSyncRefs,
            referenceDate,
            ct);

        return selectOutput(response) ?? [];
    }
}

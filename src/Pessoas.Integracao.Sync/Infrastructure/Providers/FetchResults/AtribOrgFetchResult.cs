using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Infrastructure.Providers.FetchResults;

public sealed record AtribOrgFetchResult(
    IReadOnlyList<ZhrSAtribOrgOutput> Data)
    : IZhrFetchResult;

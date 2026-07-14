using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;

namespace Pessoas.Integracao.Sync.Infrastructure.Contracts;

public interface IZhrFetcherByNi
{
    Task<IEnumerable<T>> ExecuteAsync<T>(
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        CancellationToken ct
    ) where T : ZhrSBaseModel;
}

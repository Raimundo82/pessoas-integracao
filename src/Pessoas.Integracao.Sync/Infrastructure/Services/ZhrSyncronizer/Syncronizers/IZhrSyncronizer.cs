using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.ZhrSyncronizer.Syncronizers;

public interface IZhrSyncronizer
{
    Task<ZhrSBaseModelOutput[]?> FetchAsync(
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        DateOnly? referenceDate = null,
        CancellationToken ct = default);
}

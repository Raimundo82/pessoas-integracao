using Pessoas.Integracao.Sync.Domain.Entities;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.ZhrDataProvider;

public interface IZhrDataProviderSync
{
    Task SyncZhrDataAsync(
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        DateOnly? referenceDate,
        CancellationToken ct);
}

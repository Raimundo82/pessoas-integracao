using Pessoas.Integracao.Sync.Domain.Entities;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.ZhrSyncronizer;

public interface IZhrSyncOrchestrator
{
    Task SyncZhrDataAsync(
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        DateOnly? referenceDate,
        CancellationToken ct);
}

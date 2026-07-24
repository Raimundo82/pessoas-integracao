using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Domain.Entities;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.ZhrOutputComposition.Enrichers;

public interface IZhrOutputsEnricher
{
    Task<IReadOnlyList<IZhrOutput>> EnrichAsync(
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        IReadOnlyList<IZhrOutput> zhrOutputs,
        CancellationToken ct
    );
}

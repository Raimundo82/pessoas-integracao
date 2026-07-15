using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Domain.Entities;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.ZhrOutputComposition.Enrichers;

public interface IZhrOutputsEnricher
{
    Task<IReadOnlyList<ZhrOutput>> EnrichAsync(
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        IReadOnlyList<ZhrOutput> zhrOutputs,
        CancellationToken ct
    );
}

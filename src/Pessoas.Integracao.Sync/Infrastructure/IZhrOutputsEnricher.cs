using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Domain.Entities;
namespace Pessoas.Integracao.Sync.Infrastructure;

public interface IZhrOutputsEnricher
{
    Task EnrichAsync(
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        IReadOnlyList<ZhrOutput> zhrOutputs,
        CancellationToken ct
    );
}

using Pessoas.Integracao.Sync.Domain.Entities;

namespace Pessoas.Integracao.Sync.Application.Contracts;

public interface IZhrDeltasConsumer
{
    Task OnDeltasProcessedAsync(IReadOnlyList<PessoaSyncRef> pessoaSyncRefs, CancellationToken ct);
}

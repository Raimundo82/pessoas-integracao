using Pessoas.Integracao.Sync.Domain.Entities;

namespace Pessoas.Integracao.Sync.Application.Contracts;

public interface IZhrOutputProvider
{
    Task<IReadOnlyList<IZhrOutput>> GetOutputsByNiAsync(IReadOnlyList<PessoaSyncRef> pessoaSyncRefs, CancellationToken ct);
}

using Pessoas.Integracao.Sync.Domain.Entities;

namespace Pessoas.Integracao.Sync.Application.Contracts;

public interface IZhrOutputProvider
{
    Task<IReadOnlyList<IZhrOutput>> GetOutputsBySyncRefsAsync(IReadOnlyList<PessoaSyncRef> pessoaSyncRefs, CancellationToken ct);
    Task<IReadOnlyList<IZhrOutput>> GetAllOutputsAsync(CancellationToken ct);
}

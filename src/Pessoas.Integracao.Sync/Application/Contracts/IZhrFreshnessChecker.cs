using Pessoas.Integracao.Sync.Domain.Entities;

namespace Pessoas.Integracao.Sync.Application.Contracts;

public interface IZhrFreshnessChecker
{
    Task<IReadOnlyList<PessoaSyncRef>> GetStaleRefsAsync(
        IReadOnlyList<PessoaSyncRef> refs,
        TimeSpan deltaTime,
        CancellationToken ct);
}

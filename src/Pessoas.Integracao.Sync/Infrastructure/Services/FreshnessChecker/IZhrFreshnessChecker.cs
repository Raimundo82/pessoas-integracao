using Pessoas.Integracao.Sync.Domain.Entities;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.FreshnessChecker;

public interface IZhrFreshnessChecker
{
    Task<IReadOnlyList<PessoaSyncRef>> GetStaleRefsAsync(
        IReadOnlyList<PessoaSyncRef> refs,
        TimeSpan deltaTime,
        CancellationToken ct);
}

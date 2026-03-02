using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Core.Application.Contracts;

public interface IDeltasProvider
{
    Task<IReadOnlyList<Delta>> GetDeltasAsync(CancellationToken cancellationToken);
}
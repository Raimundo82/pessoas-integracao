using Pessoas.Integracao.Analitica.Models;

namespace Pessoas.Integracao.Analitica.Application.Contracts;

public interface IAnaliticaRepository<TEntity> where TEntity : ZhrWsBaseModel
{
    Task ReplaceMatchingByNiAsync(IReadOnlyList<TEntity> entities, CancellationToken ct);
    Task ReplaceAllAsync(IReadOnlyList<TEntity> entities, CancellationToken ct);
}

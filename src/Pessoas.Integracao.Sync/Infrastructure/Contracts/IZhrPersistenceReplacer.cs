using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Infrastructure.Contracts;

public interface IZhrPersistenceReplacer
{
    Task ExecuteAsync<T>(
        IReadOnlyList<T> roots,
        IReadOnlyList<ZhrSBaseModel[]> children,
        CancellationToken ct
    ) where T : ZhrSBaseModelOutput, IOutputModel;
}

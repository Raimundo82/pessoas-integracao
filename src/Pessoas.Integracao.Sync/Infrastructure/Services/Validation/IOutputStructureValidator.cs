using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.Validation;

public interface IOutputStructureValidator
{
    Task<bool> IsValidAsync<TOutput>(IReadOnlyList<ZhrSBaseModelOutput> outputs, CancellationToken ct);
}

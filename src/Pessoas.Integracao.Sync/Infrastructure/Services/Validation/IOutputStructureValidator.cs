using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.Validation;

public interface IOutputStructureValidator
{
    bool IsValid<TOutput>(IReadOnlyList<ZhrSBaseModelOutput> outputs);
}

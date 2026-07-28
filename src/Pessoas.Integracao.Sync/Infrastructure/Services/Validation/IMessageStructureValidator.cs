using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.Validation;

public interface IMessageStructureValidator
{
    bool IsValid(IReadOnlyList<ZhrSLogMsg> logMessages);
}

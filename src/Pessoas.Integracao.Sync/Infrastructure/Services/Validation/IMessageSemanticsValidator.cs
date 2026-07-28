using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;


namespace Pessoas.Integracao.Sync.Infrastructure.Services.Validation;

public interface IMessageSemanticsValidator
{
    SapOutcome Validate(IReadOnlyList<ZhrSLogMsg> logMessages);
}

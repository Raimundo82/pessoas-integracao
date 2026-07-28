using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.Validation;

public interface IMessageSemanticsValidator
{
    Task<SapOutcome> ValidateAsync(IReadOnlyList<ZhrSLogMsg> logMessages, CancellationToken ct);
}

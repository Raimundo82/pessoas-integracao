using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.Validation;

public class ValidationResult(SapOutcome sapOutcome, ValidationFailureFlags failures, IReadOnlyCollection<ZhrSLogMsg> messages)
{
    public SapOutcome SapOutcome { get; } = sapOutcome;
    public ValidationFailureFlags Failures { get; } = failures;
    public IReadOnlyCollection<ZhrSLogMsg> Messages { get; } = messages;

    public bool IsValid => Failures == ValidationFailureFlags.None;
}

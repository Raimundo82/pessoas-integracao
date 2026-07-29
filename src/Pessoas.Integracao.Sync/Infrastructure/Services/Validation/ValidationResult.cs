using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.Validation;

public record ValidationResult(SapOutcome SapOutcome, ValidationFailures Failures, IReadOnlyCollection<ZhrSLogMsg> Messages)
{
    public SapOutcome SapOutcome { get; } = SapOutcome;
    public ValidationFailures Failures { get; } = Failures;
    public IReadOnlyCollection<ZhrSLogMsg> Messages { get; } = Messages;

    public bool IsValid => Failures == ValidationFailures.None;
}

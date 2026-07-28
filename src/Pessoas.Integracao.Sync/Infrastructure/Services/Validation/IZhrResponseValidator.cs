using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.Validation;

public interface IZhrResponseValidator
{
    Task<ValidationResult> ValidateOutputs<TExpectedOutput>(IZhrWsBaseResponse? response, CancellationToken ct);
}

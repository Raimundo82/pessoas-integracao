using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.Validation;

public interface IMessageStructureValidator
{
    Task<bool> IsValidAsync(IReadOnlyList<ZhrSLogMsg> logMessages, CancellationToken ct);
}

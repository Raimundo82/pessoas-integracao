using Pessoas.Integracao.Core.Application.Models;

namespace Pessoas.Integracao.Core.Application.Contracts;

public interface IPessoasDeltasKeyProvider
{
    Task<IReadOnlyList<PessoaDeltasKey>> GetPessoasDeltasKeysAsync(TimePeriod timePeriod, CancellationToken ct);
}
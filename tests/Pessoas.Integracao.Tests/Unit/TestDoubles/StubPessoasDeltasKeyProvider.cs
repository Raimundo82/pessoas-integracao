using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;

namespace Pessoas.Integracao.Tests.Unit.TestDoubles;

public sealed class StubPessoasDeltasKeyProvider(IReadOnlyList<PessoaDeltasKey> keys) : IPessoasDeltasKeyProvider
{
    private readonly IReadOnlyList<PessoaDeltasKey> _keys = keys;

    public Task<IReadOnlyList<PessoaDeltasKey>> GetPessoasDeltasKeysAsync(
        TimePeriod timePeriod,
        CancellationToken ct)
        => Task.FromResult(_keys);
}
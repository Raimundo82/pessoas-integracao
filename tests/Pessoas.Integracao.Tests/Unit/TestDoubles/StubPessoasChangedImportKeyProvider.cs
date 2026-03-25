using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;

namespace Pessoas.Integracao.Tests.Unit.TestDoubles;

public sealed class StubPessoasChangedImportKeyProvider(IReadOnlyList<PessoaImportKey> keys) : IPessoasChangedImportKeyProvider
{
    private readonly IReadOnlyList<PessoaImportKey> _keys = keys;

    public Task<IReadOnlyList<PessoaImportKey>> GetChangedImportKeysAsync(TimePeriod timePeriod, CancellationToken ct)
        => Task.FromResult(_keys);
}
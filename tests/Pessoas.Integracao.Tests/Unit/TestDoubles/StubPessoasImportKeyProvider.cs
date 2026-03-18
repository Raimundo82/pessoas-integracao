using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;

namespace Pessoas.Integracao.Tests.Unit.TestDoubles;

public sealed class StubPessoasImportKeyProvider(IReadOnlyList<PessoaImportKey> keys) : IPessoasImportKeyProvider
{
    private readonly IReadOnlyList<PessoaImportKey> _keys = keys;

    public Task<IReadOnlyList<PessoaImportKey>> GetSourceImportKeysAsync(CancellationToken ct)
        => Task.FromResult(_keys);
}
// tests/Pessoas.Integracao.Tests/Unit/TestDoubles/StubPessoasDataProvider.cs
using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Tests.Unit.TestDoubles;

public sealed class StubPessoasDataProvider(IReadOnlyList<Pessoa> pessoasToReturn) : IPessoasDataProvider
{
    private readonly IReadOnlyList<Pessoa> _pessoasToReturn = pessoasToReturn;

    public IReadOnlyList<PessoaImportKey>? LastRequestedKeys { get; private set; }
    public CancellationToken? LastToken { get; private set; }

    public Task<IReadOnlyList<Pessoa>> GetPessoasByImportKeysAsync(
        IReadOnlyList<PessoaImportKey> keys,
        CancellationToken ct)
    {
        LastRequestedKeys = keys;
        LastToken = ct;
        return Task.FromResult(_pessoasToReturn);
    }
}

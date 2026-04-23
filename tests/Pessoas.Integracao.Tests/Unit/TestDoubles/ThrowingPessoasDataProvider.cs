using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Tests.Unit.TestDoubles;

public sealed class ThrowingPessoasDataProvider(Exception exceptionToThrow) : IPessoasDataProvider
{
    private readonly Exception _exceptionToThrow = exceptionToThrow;
    public bool WasCalled { get; private set; }

    public Task<IReadOnlyList<Pessoa>> GetPessoasByImportKeysAsync(
        IReadOnlyList<PessoaImportKey> keys,
        CancellationToken ct)
    {
        WasCalled = true;
        return Task.FromException<IReadOnlyList<Pessoa>>(_exceptionToThrow);
    }
}

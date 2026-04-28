using System.Collections.ObjectModel;

using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Tests.Unit.TestDoubles;

public sealed class ThrowingFakePessoasRepository(IReadOnlyList<PessoaImportKey> existingKeys, Exception exceptionToThrow) : IPessoaRepository
{
    private readonly IReadOnlyList<PessoaImportKey> _existingKeys = existingKeys;
    public CancellationToken? LastGetKeysToken { get; private set; }
    private readonly Exception _exceptionToThrow = exceptionToThrow;
    public bool WasCalled { get; private set; }

    public Task<Pessoa> AddAsync(Pessoa pessoa, CancellationToken ct) => throw new NotSupportedException();

    public Task<UpsertPessoasResult> UpsertAllAsync(IReadOnlyList<Pessoa> pessoas, CancellationToken ct)
    {
        WasCalled = true;
        return Task.FromException<UpsertPessoasResult>(_exceptionToThrow);
    }

    public Task AddRangeAsync(IReadOnlyList<Pessoa> pessoas, CancellationToken ct) => throw new NotSupportedException();

    public Task ClearAllAsync(CancellationToken ct) => throw new NotSupportedException();

    public Task<IReadOnlyList<Pessoa>> GetAllAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Pessoa>>(new ReadOnlyCollection<Pessoa>([]));

    public Task<IReadOnlyList<PessoaImportKey>> GetExistingImportKeysAsync(CancellationToken cancellationToken)
    {
        LastGetKeysToken = cancellationToken;
        return Task.FromResult(_existingKeys);
    }
    public Task<IReadOnlyList<Pessoa>> GetPessoaByImportKeyAsync(
       PessoaImportKey pessoaImportKey,
       CancellationToken ct)
    {
        WasCalled = true;
        return Task.FromException<IReadOnlyList<Pessoa>>(_exceptionToThrow);
    }

    public Task<IReadOnlyList<Pessoa>> GetPessoasByNiiAsync(IReadOnlyList<string> niis, CancellationToken ct) => throw new NotSupportedException();
}

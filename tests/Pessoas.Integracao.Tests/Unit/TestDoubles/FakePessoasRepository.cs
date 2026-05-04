using System.Collections.ObjectModel;


using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Tests.Unit.TestDoubles;

public sealed class FakePessoaRepository(IReadOnlyList<PessoaImportKey> existingKeys) : IPessoaRepository
{
    private readonly IReadOnlyList<PessoaImportKey> _existingKeys = existingKeys;

    public IReadOnlyList<Pessoa>? LastUpsertedPessoas { get; private set; }
    public CancellationToken? LastGetKeysToken { get; private set; }
    public CancellationToken? LastUpsertToken { get; private set; }

    public Task<IReadOnlyList<PessoaImportKey>> GetExistingImportKeysAsync(CancellationToken cancellationToken)
    {
        LastGetKeysToken = cancellationToken;
        return Task.FromResult(_existingKeys);
    }

    public Task<UpsertPessoasResult> UpsertAllAsync(IReadOnlyList<Pessoa> pessoas, CancellationToken ct)
    {
        LastUpsertedPessoas = pessoas;
        LastUpsertToken = ct;
        return Task.FromResult(new UpsertPessoasResult(pessoas.Count, 0));
    }

    public Task<Pessoa> AddAsync(Pessoa pessoa, CancellationToken ct) => throw new NotSupportedException();
    public Task AddRangeAsync(IReadOnlyList<Pessoa> pessoas, CancellationToken ct) => throw new NotSupportedException();
    public Task ClearAllAsync(CancellationToken ct) => throw new NotSupportedException();
    public Task<IReadOnlyList<Pessoa>> GetAllAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Pessoa>>(new ReadOnlyCollection<Pessoa>([]));
    public Task<IReadOnlyList<Pessoa>> GetPessoasByNiiAsync(IReadOnlyList<string> niis, CancellationToken ct) => throw new NotSupportedException();

    public Task ReplaceAllAsync(IReadOnlyList<Pessoa> pessoas, CancellationToken ct) => throw new NotSupportedException();
}

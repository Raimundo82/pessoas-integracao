using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Core.Application.Contracts;

public interface IPessoaRepository
{
    Task<Pessoa> AddAsync(Pessoa pessoa, CancellationToken ct);
    Task AddRangeAsync(IReadOnlyList<Pessoa> pessoas, CancellationToken ct);
    Task ClearAllAsync(CancellationToken ct);
    Task<UpsertPessoasResult> UpsertAllAsync(IReadOnlyList<Pessoa> pessoas, CancellationToken ct);
    Task<IReadOnlyList<Pessoa>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<PessoaImportKey>> GetExistingImportKeysAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Pessoa>> GetPessoaByImportKeyAsync(PessoaImportKey pessoaImportKey, CancellationToken ct);
}
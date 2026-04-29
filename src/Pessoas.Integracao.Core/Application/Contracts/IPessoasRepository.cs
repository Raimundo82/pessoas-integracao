using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Core.Application.Contracts;

public interface IPessoaRepository
{
    Task<Pessoa> AddAsync(Pessoa pessoa, CancellationToken ct);
    Task AddRangeAsync(IReadOnlyList<Pessoa> pessoas, CancellationToken ct);
    Task ClearAllAsync(CancellationToken ct);
    [Obsolete("Use BulkUpsertAsync instead — benchmarks show it is 2-4x faster and uses ~15x less memory at scale.")]
    Task<UpsertPessoasResult> UpsertAllAsync(IReadOnlyList<Pessoa> pessoas, CancellationToken ct);
    Task BulkUpsertAsync(IReadOnlyList<Pessoa> pessoas, CancellationToken ct);
    Task<IReadOnlyList<Pessoa>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<PessoaImportKey>> GetExistingImportKeysAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Pessoa>> GetPessoasByNiiAsync(IReadOnlyList<string> niis, CancellationToken ct);
    [Obsolete("Use GetPessoasByNiiAsync instead — benchmarks show it is 2-6x faster and uses ~4.5x less memory at scale.")]
    Task<IReadOnlyList<Pessoa>> BulkGetPessoasByNiiAsync(IReadOnlyList<string> niis, CancellationToken ct);


}

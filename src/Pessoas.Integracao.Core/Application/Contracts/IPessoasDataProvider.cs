using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Core.Application.Contracts;

public interface IPessoasDataProvider
{
    Task<IReadOnlyList<Pessoa>> GetPessoasByImportKeysAsync(IReadOnlyList<PessoaImportKey> keys, CancellationToken ct);
}

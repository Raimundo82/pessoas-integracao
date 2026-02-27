using Pessoas.Integracao.Core.Application.Models;

namespace Pessoas.Integracao.Core.Application.Contracts;

public interface IPessoasImportKeyProvider
{
    Task<IReadOnlyList<PessoaImportKey>> GetSourceImportKeysAsync(CancellationToken ct);
}
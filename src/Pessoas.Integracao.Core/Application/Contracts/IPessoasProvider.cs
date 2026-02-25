using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Core.Application.Contracts;

public interface IPessoasProvider
{
    Task<IReadOnlyList<Pessoa>> GetPessoasByImportKeysAsync(IReadOnlyList<PessoaImportKey> pessoaImportKeys, CancellationToken cancellationToken);
    Task<IReadOnlyList<PessoaImportKey>> GetSourceImportKeysAsync(CancellationToken cancellationToken);

}
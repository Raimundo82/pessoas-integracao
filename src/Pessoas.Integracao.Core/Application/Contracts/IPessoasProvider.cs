using Pessoas.Integracao.Core.Application.DTOs;
using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Core.Application.Contracts;

public interface IPessoasProvider
{
    Task<IReadOnlyList<Pessoa>> GetPessoasAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Pessoa>> GetPessoasByNiiAsync(IReadOnlyList<ImportNiiDto> importNiis, CancellationToken cancellationToken);
    Task<IReadOnlyList<ImportNiiDto>> GetProviderImportNiisAsync(CancellationToken cancellationToken);
}
using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Core.Application.Contracts;

public interface IPessoasProvider
{
    Task<IReadOnlyList<Pessoa>> GetPessoasAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Pessoa>> GetPessoasByNiiAsync(IReadOnlyList<Pessoa> pessoas, CancellationToken cancellationToken);
}
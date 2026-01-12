using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Core.Application.Contracts;

public interface IPessoasProvider
{
    Task<IReadOnlyCollection<Pessoa>> GetPessoasAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Pessoa>> GetPessoasByNiiAsync(IReadOnlyCollection<Pessoa> pessoas, CancellationToken cancellationToken);
}
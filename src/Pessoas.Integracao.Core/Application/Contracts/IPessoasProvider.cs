using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Core.Application.Contracts;

public interface IPessoasProvider
{
    Task<IReadOnlyCollection<Pessoa>> GetPessoasAsync(CancellationToken cancellationToken);
}
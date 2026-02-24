using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Core.Application.Contracts;

public interface IPessoasProvider
{
    Task<IReadOnlyList<Pessoa>> GetPessoasAsync(CancellationToken cancellationToken);
}
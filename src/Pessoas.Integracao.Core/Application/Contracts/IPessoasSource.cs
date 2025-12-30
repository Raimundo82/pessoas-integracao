using System.Collections.ObjectModel;

using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Core.Application.Contracts;

public interface IPessoasSource
{
    Task<ReadOnlyCollection<Pessoa>> GetPessoasAsync(CancellationToken cancellationToken);
}
using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Core.Application.Contracts;

public interface IPessoasDeltaDetector
{
    Task<bool> IsPessoaChangedAsync(Pessoa p1, Pessoa p2, CancellationToken ct);
}
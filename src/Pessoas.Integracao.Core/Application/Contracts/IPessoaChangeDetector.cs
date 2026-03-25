using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Core.Application.Contracts;

public interface IPessoaChangeDetector
{
    bool IsPessoaChanged(Pessoa p1, Pessoa p2);
}
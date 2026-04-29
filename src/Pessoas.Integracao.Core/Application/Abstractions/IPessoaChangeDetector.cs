using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Core.Application.Abstractions;

public interface IPessoaChangeDetector
{
    PessoaChangeResult GetChanges(Pessoa current, Pessoa? previous);
}

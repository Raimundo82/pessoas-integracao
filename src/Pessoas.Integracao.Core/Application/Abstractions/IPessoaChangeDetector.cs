using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Core.Application.Abstractions;

public interface IPessoaChangeDetector
{
    // Rules:
    // - If previous is null → return { Added }
    // - PessoaAdded is exclusive (cannot be combined with other values)
    // - No duplicate change types
    IReadOnlySet<PessoaChangeType> GetChanges(Pessoa current, Pessoa? previous);
}

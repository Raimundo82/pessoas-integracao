namespace Pessoas.Integracao.Core.Application.Models;

public sealed record PessoaChangeResult(
    // Rules:
    // - If existing pessoa is null → return { Added }
    // - Added is exclusive (cannot be combined with other values)
    // - No duplicate change types
    IReadOnlySet<PessoaChangeType> ChangeTypes)
{
    public bool HasChanges => ChangeTypes != null && ChangeTypes.Count > 0;
}

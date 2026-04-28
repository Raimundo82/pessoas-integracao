namespace Pessoas.Integracao.Core.Application.Models;

public sealed record PessoaChangeResult(
    bool HasChanges,
    IReadOnlySet<PessoaChangeType> ChangeTypes);

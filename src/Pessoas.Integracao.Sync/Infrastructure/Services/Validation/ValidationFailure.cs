namespace Pessoas.Integracao.Sync.Infrastructure.Services.Validation;

[Flags]
public enum ValidationFailure
{
    None = 0,
    MessageStructure = 1,
    OutputStructure = 2,
    Semantics = 4
}

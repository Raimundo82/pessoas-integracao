using Pessoas.Integracao.Core.Domain.Enums;

namespace Pessoas.Integracao.Core.Domain.ValueObjects;

public class TipoDeSangue
{
    public GrupoSanguineo? GrupoSanguineo { get; init; }
    public Rhesus? Rhesus { get; init; }

}
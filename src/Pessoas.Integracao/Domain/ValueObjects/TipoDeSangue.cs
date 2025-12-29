using Pessoas.Integracao.Domain.Enums;

namespace Pessoas.Integracao.Domain.ValueObjects;

public class TipoDeSangue
{
    public GrupoSanguineo? GrupoSanguineo { get; init; }
    public Rhesus? Rhesus { get; init; }

}
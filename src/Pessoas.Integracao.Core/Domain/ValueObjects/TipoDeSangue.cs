using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Core.Domain.Enums;

namespace Pessoas.Integracao.Core.Domain.ValueObjects;

[Owned]
public class TipoDeSangue
{
    public GrupoSanguineo? GrupoSanguineo { get; init; }
    public Rhesus? Rhesus { get; init; }

}
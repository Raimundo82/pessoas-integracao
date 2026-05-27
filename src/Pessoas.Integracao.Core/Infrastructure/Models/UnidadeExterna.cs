using Microsoft.EntityFrameworkCore;

namespace Pessoas.Integracao.Core.Infrastructure.Models;

[Index(nameof(ExternalId), nameof(ValidFrom), IsUnique = true)]
public class UnidadeExterna
{
    public int Id { get; init; }
    public required string ExternalId { get; init; }
    public string? Descricao { get; init; }
    public string? Abreviatura { get; init; }
    public DateTimeOffset ValidFrom { get; init; }
    public DateTimeOffset? ValidTo { get; init; }
    public bool IsCurrent { get; init; }

    public bool HasAttributeChanges(string? descricao, string? abreviatura)
    {
        return !string.Equals(Descricao, descricao, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(Abreviatura, abreviatura, StringComparison.OrdinalIgnoreCase);
    }

    public UnidadeExterna CloseVersion(DateTimeOffset changedAt)
    {
        return new UnidadeExterna
        {
            Id = Id,
            ExternalId = ExternalId,
            Descricao = Descricao,
            Abreviatura = Abreviatura,
            ValidFrom = ValidFrom,
            ValidTo = changedAt,
            IsCurrent = false,
        };
    }

    public UnidadeExterna CreateNextVersion(string? descricao, string? abreviatura, DateTimeOffset changedAt)
    {
        return new UnidadeExterna
        {
            ExternalId = ExternalId,
            Descricao = descricao,
            Abreviatura = abreviatura,
            ValidFrom = changedAt,
            ValidTo = null,
            IsCurrent = true,
        };
    }
}

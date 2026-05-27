using Microsoft.EntityFrameworkCore;

namespace Pessoas.Integracao.Core.Infrastructure.Models;

[Index(nameof(ExternalId), nameof(ValidFrom), IsUnique = true)]
public class UnidadeExterna
{
    public int Id { get; set; }
    public required string ExternalId { get; set; }
    public string? Descricao { get; set; }
    public string? Abreviatura { get; set; }
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }
    public bool IsCurrent { get; set; }

    public bool HasAttributeChanges(string? descricao, string? abreviatura)
    {
        return !string.Equals(Descricao, descricao, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(Abreviatura, abreviatura, StringComparison.OrdinalIgnoreCase);
    }

    public void CloseVersion(DateTimeOffset changedAt)
    {
        ValidTo = changedAt;
        IsCurrent = false;
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

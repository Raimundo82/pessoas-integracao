using Microsoft.EntityFrameworkCore;

namespace Pessoas.Integracao.Core.Domain.Entities;

[Index(nameof(NII), IsUnique = true)]
public class Delta
{
    public int Id { get; set; }
    public required string NII { get; init; }
    public string? ExternalId { get; set; }
}
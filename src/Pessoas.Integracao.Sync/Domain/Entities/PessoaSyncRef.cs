using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Sync.Domain.ValueObjects;

namespace Pessoas.Integracao.Sync.Domain.Entities;


[Index(nameof(Ni), IsUnique = true)]
public class PessoaSyncRef
{
    public required string Ni { get; set; }
    public required string ExternalId { get; set; }
    public required SyncState SyncState { get; set; }
}

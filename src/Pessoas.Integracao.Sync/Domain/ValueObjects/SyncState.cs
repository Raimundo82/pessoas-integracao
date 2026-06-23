using Microsoft.EntityFrameworkCore;

namespace Pessoas.Integracao.Sync.Domain.ValueObjects;

[Owned]
public sealed class SyncState
{
    public DateTimeOffset? UpdatedAt { get; set; }
}

using Microsoft.EntityFrameworkCore;

namespace Pessoas.Integracao.Sync.Domain.ValueObjects;

[Owned]
public sealed record SyncState(DateTimeOffset UpdatedAt);

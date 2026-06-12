using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Worker.Domain.ValueObjects;

namespace Pessoas.Integracao.Worker.Domain.Entities;


[Index(nameof(Ni), IsUnique = true)]
public class ImportKeySyncState
{
    public required string Ni { get; set; }
    public required string Numsap { get; set; }
    public required SyncState SyncState { get; set; }
}

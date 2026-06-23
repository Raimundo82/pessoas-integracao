using System.ComponentModel.DataAnnotations;

using Pessoas.Integracao.Sync.Domain.ValueObjects;

namespace Pessoas.Integracao.Sync.Domain.Entities;

public class PessoaSyncRef
{
    [Key]
    public required string Ni { get; set; }
    public required string ExternalId { get; set; }
    public required SyncState SyncState { get; set; } = new SyncState();
}

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Domain.ValueObjects;

namespace Pessoas.Integracao.Sync.Infrastructure.Data;

public class PessoaSyncRefDbContext(DbContextOptions<PessoaSyncRefDbContext> options) : DbContext(options)
{
    public DbSet<PessoaSyncRef> PessoaSyncRefs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PessoaSyncRef>(entity =>
        {
            entity.HasKey(e => e.Ni);

            entity.Property(e => e.Ni)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.ExternalId)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.SyncState)
                .HasConversion(
                    v => v.UpdatedAt,
                    v => new SyncState(v));
        });
    }
}

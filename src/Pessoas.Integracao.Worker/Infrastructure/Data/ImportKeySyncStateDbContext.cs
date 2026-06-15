using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Worker.Domain.Entities;
using Pessoas.Integracao.Worker.Domain.ValueObjects;

namespace Pessoas.Integracao.Worker.Infrastructure.Data;

public class ImportKeySyncStateDbContext(DbContextOptions<ImportKeySyncStateDbContext> options) : DbContext(options)
{
    public DbSet<ImportKeySyncState> ImportKeySyncStates { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ImportKeySyncState>(entity =>
        {
            entity.HasKey(e => e.Ni);

            entity.Property(e => e.Ni)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Numsap)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.SyncState)
                .HasConversion(
                    v => v.UpdatedAt,
                    v => new SyncState(v))
                .HasColumnName("sync_state_updated_at");
        });
    }
}

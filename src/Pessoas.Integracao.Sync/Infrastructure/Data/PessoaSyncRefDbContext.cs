using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Sync.Domain.Entities;

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

            entity.OwnsOne(e => e.SyncState, navigation =>
            {
                navigation.Property(s => s.UpdatedAt)
                    .HasColumnName("SyncState")
                    .HasColumnType("timestamp with time zone")
                    .IsRequired();
            });
        });
    }
}

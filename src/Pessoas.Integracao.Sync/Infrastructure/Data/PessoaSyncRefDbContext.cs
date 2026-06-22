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
            entity
                .Property(e => e.ExternalId)
                .IsRequired()
                .HasMaxLength(255));
    }
}

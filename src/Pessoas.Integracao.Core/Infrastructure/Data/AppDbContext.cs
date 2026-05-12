using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.ValueObjects;
using Pessoas.Integracao.Core.Infrastructure.Persistence;

namespace Pessoas.Integracao.Core.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Pessoa> Pessoas { get; set; }
    public DbSet<Colocacao> Colocacoes { get; set; }
    public DbSet<UnidadeExterna> UnidadesExternas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Colocacao>()
            .Property(c => c.UnidadeExternaRef)
            .HasConversion(
                v => v.ExternalReference,
                v => new UnidadeExternaRef(v));

        modelBuilder.Entity<UnidadeExterna>()
            .HasIndex(u => u.ExternalId)
            .IsUnique();
    }
}

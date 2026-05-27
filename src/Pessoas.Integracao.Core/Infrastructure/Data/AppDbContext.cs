using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.ValueObjects;
using Pessoas.Integracao.Core.Infrastructure.Models;

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
            .Property(c => c.ExternalReference)
            .HasConversion(
                v => v.ExternalReference,
                v => new UnidadeExternaRef(v));
    }
}

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Worker.Infrastructure.Models.Dados;

namespace Pessoas.Integracao.Worker.Infrastructure.Data;

public class ZhrSDbContext(DbContextOptions<ZhrSDbContext> options) : DbContext(options)
{
    public DbSet<ZhrSAptidaoOutput> ZhrSAptidaoOutputs { get; set; } = null!;
    public DbSet<ZhrSAptidao> ZhrSAptidoes { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ZhrSAptidaoOutput>(e => e.HasIndex(a => a.Ni));
    }
}

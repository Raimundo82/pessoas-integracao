using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Domain.Entities;

namespace Pessoas.Integracao.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Pessoa> Pessoas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Pessoa>()
            .HasIndex(p => p.NII)
            .IsUnique();

        modelBuilder.Entity<Pessoa>()
            .OwnsOne(p => p.DadosPessoais);

        modelBuilder.Entity<Pessoa>()
            .OwnsOne(p => p.DadosBiometricos, db =>
                db.OwnsOne(b => b.TipoDeSangue));
    }

}
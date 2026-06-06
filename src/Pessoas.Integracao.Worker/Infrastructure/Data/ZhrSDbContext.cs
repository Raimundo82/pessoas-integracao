using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Worker.Infrastructure.Models.Dados;

namespace Pessoas.Integracao.Worker.Infrastructure.Data;

public class ZhrSDbContext(DbContextOptions<ZhrSDbContext> options) : DbContext(options)
{
    // Aptidao
    public DbSet<ZhrSAptidaoOutput> ZhrSAptidaoOutputs { get; set; } = null!;
    public DbSet<ZhrSAptidao> ZhrSAptidoes { get; set; } = null!;

    // AtribOrg
    public DbSet<ZhrSAtribOrgOutput> ZhrSAtribOrgOutputs { get; set; } = null!;
    public DbSet<ZhrSAtribOrg> ZhrSAtribOrgs { get; set; } = null!;
    public DbSet<ZhrSClassifProf> ZhrSClassifProfs { get; set; } = null!;
    public DbSet<ZhrSDataMedida> ZhrSDataMedidas { get; set; } = null!;
    public DbSet<ZhrSInfoProm> ZhrSInfoProms { get; set; } = null!;
    public DbSet<ZhrSMonitPrazos> ZhrSMonitPrazos { get; set; } = null!;
    public DbSet<ZhrSOm> ZhrSOms { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var outputSubclasses = modelBuilder.Model
            .GetEntityTypes()
            .Where(t => t.ClrType.IsSubclassOf(typeof(ZhrSBaseModelOutput)));

        var requiredAndIndexedProps = new[] { "Ni", "Numsap" };

        foreach (var entityType in outputSubclasses)
        {
            var entity = modelBuilder.Entity(entityType.ClrType);

            foreach (var propName in requiredAndIndexedProps)
            {
                entity.Property(propName).IsRequired();
                entity.HasIndex(propName).IsUnique();
            }
        }
    }
}

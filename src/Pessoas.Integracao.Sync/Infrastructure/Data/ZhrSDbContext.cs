using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Infrastructure.Data;

public class ZhrSDbContext(DbContextOptions<ZhrSDbContext> options) : DbContext(options)
{
    // Aptidao
    public DbSet<ZhrSAptidaoOutput> ZhrSAptidaoOutputs { get; set; } = null!;
    public DbSet<ZhrSAptidao> ZhrSAptidoes { get; set; } = null!;

    // Pessoais
    public DbSet<ZhrSPessoaisOutput> ZhrSPessoaisOutputs { get; set; } = null!;
    public DbSet<ZhrSPessoais> ZhrSPessoais { get; set; } = null!;
    public DbSet<ZhrSFamilia> ZhrSFamilias { get; set; } = null!;
    public DbSet<ZhrSOutrosdados> ZhrSOutrosdados { get; set; } = null!;
    public DbSet<ZhrSDeficiencias> ZhrSDeficiencias { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var outputTypes = modelBuilder.Model
            .GetEntityTypes()
            .Where(t => t.ClrType.IsSubclassOf(typeof(ZhrSBaseModelOutput)));

        foreach (var outputEntityType in outputTypes.Select(t => t.ClrType))
        {
            var outputEntity = modelBuilder.Entity(outputEntityType);
            outputEntity.HasKey(nameof(IOutputModel.Ni));
            outputEntity.Property(nameof(IOutputModel.Numsap)).IsRequired();
            outputEntity.HasIndex(nameof(IOutputModel.Numsap)).IsUnique();

            var props = outputEntityType.GetProperties();

            foreach (var prop in props)
            {
                if (prop.PropertyType.IsArray)
                {
                    var elementType = prop.PropertyType.GetElementType();

                    if (elementType != null && elementType.IsSubclassOf(typeof(ZhrSBaseModel)))
                    {
                        modelBuilder.Entity(elementType)
                            .HasOne(outputEntityType)
                            .WithMany()
                            .HasForeignKey(nameof(ZhrSBaseModel.Ni))
                            .OnDelete(DeleteBehavior.Cascade);
                    }
                    outputEntity.Ignore(prop.Name);
                }
            }
        }
    }
}

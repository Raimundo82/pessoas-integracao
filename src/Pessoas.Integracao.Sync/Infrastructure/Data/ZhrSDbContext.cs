using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Sync.Infrastructure.Models.Dados;

namespace Pessoas.Integracao.Sync.Infrastructure.Data;

public class ZhrSDbContext(DbContextOptions<ZhrSDbContext> options) : DbContext(options)
{
    // Aptidao
    public DbSet<ZhrSAptidaoOutput> ZhrSAptidaoOutputs { get; set; } = null!;
    public DbSet<ZhrSAptidao> ZhrSAptidoes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var outputTypes = modelBuilder.Model
            .GetEntityTypes()
            .Where(t => t.ClrType.IsSubclassOf(typeof(ZhrSBaseModelOutput)))
            .ToList();


        foreach (var outputEntityType in outputTypes)
        {
            var outputEntity = modelBuilder.Entity(outputEntityType.ClrType);
            outputEntity.HasKey("Ni");
            outputEntity.Property("Numsap").IsRequired();
            outputEntity.HasIndex("Numsap").IsUnique();

            var props = outputEntityType.ClrType.GetProperties();
            foreach (var prop in props)
            {
                if (prop.PropertyType.IsArray)
                {
                    var elementType = prop.PropertyType.GetElementType();

                    if (elementType != null && elementType.IsSubclassOf(typeof(ZhrSBaseModel)))
                    {
                        outputEntity
                            .HasMany(elementType, prop.Name)
                            .WithOne("Output")
                            .HasForeignKey("Ni");

                    }
                    else
                    {
                        outputEntity.Ignore(prop.Name);
                    }
                }
            }
        }
    }
}

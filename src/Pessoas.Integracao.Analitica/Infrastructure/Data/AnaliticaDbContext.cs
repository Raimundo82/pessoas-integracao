
using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Analitica.Models;

namespace Pessoas.Integracao.Analitica.Infrastructure.Data;

public partial class AnaliticaDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
            .Where(t =>
                typeof(IAnaliticaModel).IsAssignableFrom(t.ClrType) &&
                t.ClrType.IsClass &&
                !t.ClrType.IsAbstract
                )
            )
        {
            modelBuilder.Entity(entityType.ClrType, entity =>
            {
                entity.HasKey("Id");
                entity.Property("Id")
                    .ValueGeneratedOnAdd();
                entity.HasIndex("Ni");
                entity.Property("UpdatedAt")
                    .HasColumnName("updated_at");
            });
        }
    }
}

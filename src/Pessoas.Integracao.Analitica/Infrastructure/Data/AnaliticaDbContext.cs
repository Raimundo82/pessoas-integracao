
using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Analitica.Models;

namespace Pessoas.Integracao.Analitica.Infrastructure.Data;

public partial class AnaliticaDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
            .Where(t => t.ClrType.IsSubclassOf(typeof(ZhrWsBaseModel))))
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

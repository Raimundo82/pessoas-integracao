using Microsoft.EntityFrameworkCore;
using SigdnRhStaggingApi.Models;

namespace SigdnRhStaggingApi.Data;

public class RhStaggingDbContext(DbContextOptions<RhStaggingDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>()
            .HasIndex(employee => employee.Ni)
            .IsUnique();

        modelBuilder.Entity<Employee>()
            .HasIndex(employee => employee.Numsap)
            .IsUnique();

    }

}

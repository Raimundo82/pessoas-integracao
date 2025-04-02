using Microsoft.EntityFrameworkCore;

namespace SigdnRhStaggingApi.Data;

public class RhStaggingDbContext(DbContextOptions<RhStaggingDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Users { get; set; }

}

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Core.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Pessoa> Pessoas { get; set; }

}

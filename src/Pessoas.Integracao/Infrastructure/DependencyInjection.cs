using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Infrastructure.Data;

namespace Pessoas.Integracao.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        builder.Services.AddScoped<DbInitialiser>();
    }

}
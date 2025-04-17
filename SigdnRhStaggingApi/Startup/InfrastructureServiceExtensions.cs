using Microsoft.EntityFrameworkCore;
using SigdnRhStaggingApi.Data;

namespace SigdnRhStaggingApi.Startup;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddDbContextFactory<RhStaggingDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
    }

}

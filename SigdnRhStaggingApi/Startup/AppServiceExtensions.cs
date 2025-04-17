using SigdnRhStaggingApi.Services;

namespace SigdnRhStaggingApi.Startup;


public static class AppServiceExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        return services.AddScoped<IEmployeeService, EmployeeService>();
    }

}

using SigdnRhStaggingApi.Services;
using SigdnRhStaggingApi.Settings;

namespace SigdnRhStaggingApi.Startup;


public static class AppServiceExtensions
{
    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration configuration)
    {
        return services.Configure<AppSettingsOptions>(configuration.GetSection(AppSettingsOptions.AppSettings));
    }

    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        return services.AddScoped<IEmployeeService, EmployeeService>();
    }

}
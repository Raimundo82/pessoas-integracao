namespace SigdnRhStaggingApi.Startup;

using Keycloak.AuthServices.Authentication;

public static class AuthServiceExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddKeycloakWebApiAuthentication(configuration, options =>
        {
            options.BackchannelHttpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
        });

        return services;
    }

}

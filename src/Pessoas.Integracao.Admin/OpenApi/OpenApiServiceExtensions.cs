
using Keycloak.AuthServices.Authentication;

using NSwag.Generation.Processors.Security;

namespace Pessoas.Integracao.Admin.OpenApi;

public static class OpenApiServiceExtensions
{
    public static IServiceCollection AddOpenApiWithAuthentication(this IServiceCollection services, KeycloakAuthenticationOptions keycloakOptions)
    {
        services.AddOpenApiDocument(options =>
        {
            options.AddSecurity("OpenIdConnect", [], new NSwag.OpenApiSecurityScheme
            {
                Type = NSwag.OpenApiSecuritySchemeType.OpenIdConnect,
                OpenIdConnectUrl = $"{keycloakOptions.AuthServerUrl}/realms/{keycloakOptions.Realm}/.well-known/openid-configuration"
            });
            options.OperationProcessors.Add(new OperationSecurityScopeProcessor("OpenIdConnect"));
        });
        return services;
    }

    public static IApplicationBuilder UseSwaggerWithOAuth(this IApplicationBuilder app, KeycloakAuthenticationOptions keycloakOptions)
    {
        app.UseSwaggerUi(options => options.OAuth2Client = new()
        {
            ClientId = keycloakOptions.Resource,
            AppName = "Plataforma de Integração de Pessoas",
            UsePkceWithAuthorizationCodeGrant = true,
            ClientSecret = keycloakOptions.Credentials.Secret,
            Realm = keycloakOptions.Realm
        });

        return app;
    }
}
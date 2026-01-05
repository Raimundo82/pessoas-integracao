using Microsoft.AspNetCore.Authentication;

using Pessoas.Integracao.Core.Domain.Constants;

namespace Pessoas.Integracao.Admin.Authorization;

public static class AuthorizationServiceExtensions
{
    public static IServiceCollection AddApplicationAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.CanImportPessoas, policy => policy.RequireRole(Roles.Admin))
            .AddPolicy(Policies.CanReadPessoas, policy => policy.RequireRole(Roles.Admin, Roles.Viewer));

        services.AddTransient<IClaimsTransformation, IdentityProviderRolesClaimsTransformation>();

        return services;
    }
}
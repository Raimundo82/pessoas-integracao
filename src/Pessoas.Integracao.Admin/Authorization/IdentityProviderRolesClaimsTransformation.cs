using System.Security.Claims;
using System.Text.Json;

using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Common;

using Microsoft.AspNetCore.Authentication;

using Pessoas.Integracao.Core.Application.Security;

namespace Pessoas.Integracao.Admin.Authorization;

public partial class IdentityProviderRolesClaimsTransformation(ILogger<IdentityProviderRolesClaimsTransformation> logger, IConfiguration configuration) : IClaimsTransformation
{
    private readonly KeycloakAuthenticationOptions? _keycloakOptions = configuration.GetKeycloakOptions<KeycloakAuthenticationOptions>();

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return Task.FromResult(principal);
        }

        var claimsIdentity = new ClaimsIdentity();
        var resourceAccessClaim = principal.FindFirst("resource_access");

        if (resourceAccessClaim is null)
        {
            LogNoResourceAccessClaim();
            principal.AddIdentity(claimsIdentity);
            return Task.FromResult(principal);
        }

        var clientId = _keycloakOptions?.Resource ?? "pessoas-integracao-test";
        var keycloakRoles = ExtractKeycloakRoles(resourceAccessClaim.Value, clientId);

        foreach (var keycloakRole in keycloakRoles)
        {
            var coreRole = Roles.FromExternalProvider(keycloakRole);
            if (coreRole is not null)
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, coreRole));
                LogMappedRole(keycloakRole, coreRole);
            }
            else
            {
                LogUnknownRole(keycloakRole);
            }
        }

        principal.AddIdentity(claimsIdentity);
        return Task.FromResult(principal);
    }

    private IEnumerable<string> ExtractKeycloakRoles(string resourceAccessJson, string clientId)
    {
        try
        {
            var resourceAccess = JsonDocument.Parse(resourceAccessJson);
            if (resourceAccess.RootElement.TryGetProperty(clientId, out var clientAccess) &&
                clientAccess.TryGetProperty("roles", out var roles))
            {
                return roles.EnumerateArray()
                    .Select(role => role.GetString())
                    .Where(role => !string.IsNullOrEmpty(role))
                    .Cast<string>();
            }
        }
        catch (JsonException ex)
        {
            LogJsonParseError(ex);
        }

        return [];
    }

    [LoggerMessage(LogLevel.Information, "Mapped Keycloak '{keycloakRole}' to Core role '{coreRole}'")]
    partial void LogMappedRole(string keycloakRole, string coreRole);

    [LoggerMessage(LogLevel.Warning, "Keycloak role '{role}' is not a Core role, skipping")]
    partial void LogUnknownRole(string role);

    [LoggerMessage(LogLevel.Warning, "No resource_access claim found in token")]
    partial void LogNoResourceAccessClaim();

    [LoggerMessage(LogLevel.Error, "Failed to parse resource_access claim")]
    partial void LogJsonParseError(Exception ex);
}

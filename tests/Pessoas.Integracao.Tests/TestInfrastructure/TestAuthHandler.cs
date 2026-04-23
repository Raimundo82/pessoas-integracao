using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Pessoas.Integracao.Tests.TestInfrastructure;

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string AuthenticationScheme = "TestScheme";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Test-Claims", out var claimsJson))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claimsData = JsonSerializer.Deserialize<List<ClaimData>>(claimsJson.ToString());
        if (claimsData is null || claimsData.Count == 0)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = claimsData.Select(c => new Claim(c.Type, c.Value)).ToList();
        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private record ClaimData(string Type, string Value);
}

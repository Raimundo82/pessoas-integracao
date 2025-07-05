using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SigdnRhStaggingApi.Tests.GraphqlTesting;

public static class TestHttpContextFactory
{
    public static HttpContext CreateAuthenticatedContext(string userName)
    {
        var claims = new[] { new Claim(ClaimTypes.Name, userName), new Claim("employeeId", userName[1..]) };
        var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        return new DefaultHttpContext
        {
            User = principal
        };
    }
}
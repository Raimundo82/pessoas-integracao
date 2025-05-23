using HotChocolate.Resolvers;
using Microsoft.Extensions.Options;
using SigdnRhStaggingApi.Settings;

namespace SigdnRhStaggingApi.Graphql;

public enum ApiKeyAccess
{
    READ,
    WRITE
}

public class ApiKeyMiddleware(FieldDelegate next, IOptions<AppSettingsOptions> appSettingsOptions, ApiKeyAccess access)
{
    private readonly FieldDelegate _next = next;
    private readonly IOptions<AppSettingsOptions> _appSettingsOptions = appSettingsOptions;
    public ApiKeyAccess Access { get; } = access;
    private const string ApiKeyHeader = "X-API-KEY";

    public async Task InvokeAsync(IMiddlewareContext context)
    {
        var httpContext = context.Services.GetRequiredService<IHttpContextAccessor>().HttpContext;
        if (httpContext == null)
        {
            if (!_appSettingsOptions.Value.AllowMissingHttpContext)
            {
                Reject(context);
                return;
            }
            await _next(context);
            return;

        }

        if (!httpContext.Request.Headers.TryGetValue(ApiKeyHeader, out var key))
        {
            Reject(context);
            return;
        }
        if (!IsAuthorized(key))
        {
            Reject(context);
            return;
        }
        await _next(context);

    }

    private bool IsAuthorized(string? key)
    {
        return Access switch
        {
            ApiKeyAccess.READ => key == _appSettingsOptions.Value.ReadApiKey || key == _appSettingsOptions.Value.WriteApiKey,
            ApiKeyAccess.WRITE => key == _appSettingsOptions.Value.WriteApiKey,
            _ => false
        };
    }

    private static void Reject(IMiddlewareContext context)
    {
        context.Result = null;
        context.ReportError(GraphQLErrorHelper.Unauthorized());
    }
}
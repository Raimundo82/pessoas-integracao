using HotChocolate.Resolvers;

namespace SigdnRhStaggingApi.Graphql;

public class OwnershipAuthorizationMiddleware(FieldDelegate next, IHttpContextAccessor httpContextAccessor, string argumentName)
{
    private readonly FieldDelegate _next = next;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly string _argumentName = argumentName;


    public async Task InvokeAsync(IMiddlewareContext context)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var argValue = context.ArgumentValue<string>(_argumentName);

        if (httpContext == null)
        {
            context.ReportError(new UnauthorizedException($"User {argValue} is not authenticated."));
            return;
        }

        var user = httpContext.User;
        if (user.Identity != null)
        {
            var employeeId = user.Claims.First(claim => claim.Type == "employeeId").Value;
            if (employeeId != null && !employeeId.Equals(argValue, StringComparison.OrdinalIgnoreCase))
            {
                context.ReportError(new ForbiddenException($"User does not have permission to access {argValue} resouce."));
                return;
            }
        }

        await _next(context);
    }
}
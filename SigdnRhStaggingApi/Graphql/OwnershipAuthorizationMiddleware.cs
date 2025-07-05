using HotChocolate.Resolvers;
using SigdnRhStaggingApi.Services;

namespace SigdnRhStaggingApi.Graphql;

public class OwnershipAuthorizationMiddleware(FieldDelegate next, ICurrentUserService currentUserService, string argumentName)
{
    private readonly FieldDelegate _next = next;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly string _argumentName = argumentName;


    public async Task InvokeAsync(IMiddlewareContext context)
    {
        var user = _currentUserService.User;
        var argValue = context.ArgumentValue<string>(_argumentName);

        if (user == null)
        {
            context.ReportError(new UnauthorizedException($"User {argValue} is not authenticated."));
            return;
        }
        var employeeId = _currentUserService.EmployeeId;
        if (employeeId != null && !employeeId.Equals(argValue, StringComparison.OrdinalIgnoreCase))
        {
            context.ReportError(new ForbiddenException($"User does not have permission to access {argValue} resouce."));
            return;
        }

        await _next(context);
    }
}
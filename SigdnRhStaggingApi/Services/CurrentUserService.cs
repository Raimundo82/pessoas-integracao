using System.Security.Claims;

namespace SigdnRhStaggingApi.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly ClaimsPrincipal? _user = httpContextAccessor.HttpContext?.User;

    public ClaimsPrincipal? User => _user;

    public string? Username => _user?.Claims.First(c => c.Type == ClaimTypes.Name).Value;

    public string? EmployeeId => _user?.Claims.First(c => c.Type == "employeeId").Value;

}
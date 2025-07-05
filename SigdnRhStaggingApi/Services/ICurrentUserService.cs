using System.Security.Claims;

namespace SigdnRhStaggingApi.Services;

public interface ICurrentUserService
{
    ClaimsPrincipal? User { get; }
    string? Username { get; }
    string? EmployeeId { get; }
}
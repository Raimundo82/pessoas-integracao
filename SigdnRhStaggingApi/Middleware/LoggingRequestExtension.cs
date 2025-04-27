
namespace SigdnRhStaggingApi.Middleware;

public static class LoggingRequestExtension
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LoggingRequestMiddleware>();
    }
}
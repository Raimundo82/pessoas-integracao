using System.Diagnostics;

namespace SigdnRhStaggingApi.Middleware;

public class LoggingRequestMiddleware(RequestDelegate next, ILogger<LoggingRequestMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<LoggingRequestMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Handling request: {Path}", context.Request.Path);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("Finished handling request: {Path}. Duration: {Duration}ms", context.Request.Path, stopwatch.ElapsedMilliseconds);
        }
    }
}

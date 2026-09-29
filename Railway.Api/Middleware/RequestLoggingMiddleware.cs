using System.Diagnostics;

namespace Railway.Api.Middleware;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var logLevel = context.Response.StatusCode >= 500 ? LogLevel.Error : LogLevel.Information;

            _logger.Log(logLevel, "HTTP {Method} {Path}{QueryString} responded {StatusCode} in {ElapsedMilliseconds} ms", context.Request.Method, context.Request.Path, context.Request.QueryString, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}
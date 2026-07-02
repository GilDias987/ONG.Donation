using Serilog;
using System.Diagnostics;

namespace ONG.Donation.WebAPI.Middlewares;

public class RequestLoggingMiddleware
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
        var correlationId = Activity.Current?.Id ?? context.TraceIdentifier;
        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userEmail = context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var userRole = context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        context.Response.Headers["X-Correlation-Id"] = correlationId;

        _logger.LogInformation(
            "HTTP {Method} {Path} initiated. CorrelationId: {CorrelationId}, UserId: {UserId}, UserEmail: {UserEmail}, Role: {Role}",
            context.Request.Method, context.Request.Path, correlationId, userId, userEmail, userRole);

        try
        {
            await _next(context);
            stopwatch.Stop();

            _logger.LogInformation(
                "HTTP {Method} {Path} completed with {StatusCode} in {ElapsedMs}ms. CorrelationId: {CorrelationId}",
                context.Request.Method, context.Request.Path, context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds, correlationId);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "HTTP {Method} {Path} failed after {ElapsedMs}ms. CorrelationId: {CorrelationId}",
                context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds, correlationId);
            throw;
        }
    }
}

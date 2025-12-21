using System.Collections.Concurrent;

namespace RemoteControl.Web.Middleware;

/// <summary>
/// Simple in-memory rate limiting middleware
/// Limits requests per IP address
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    
    // Store request counts per IP: IP -> (count, windowStart)
    private static readonly ConcurrentDictionary<string, (int Count, DateTime WindowStart)> _requestCounts = new();
    
    // Config
    private const int AuthLimit = 5;         // 5 auth attempts per minute
    private const int ApiLimit = 100;        // 100 API calls per minute
    private const int WindowSeconds = 60;    // 1 minute window

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        // Determine limit based on path
        int limit;
        string limitType;
        
        if (path.StartsWith("/api/auth"))
        {
            limit = AuthLimit;
            limitType = "auth";
        }
        else if (path.StartsWith("/api") || path.StartsWith("/remotehub"))
        {
            limit = ApiLimit;
            limitType = "api";
        }
        else
        {
            // No rate limit for static files and pages
            await _next(context);
            return;
        }
        
        var key = $"{ip}:{limitType}";
        var now = DateTime.UtcNow;
        
        // Get or create rate limit entry
        var entry = _requestCounts.GetOrAdd(key, _ => (0, now));
        
        // Check if window expired
        if ((now - entry.WindowStart).TotalSeconds >= WindowSeconds)
        {
            // Reset window
            entry = (1, now);
            _requestCounts[key] = entry;
        }
        else
        {
            // Increment count
            entry = (entry.Count + 1, entry.WindowStart);
            _requestCounts[key] = entry;
        }
        
        // Check if over limit
        if (entry.Count > limit)
        {
            _logger.LogWarning("Rate limit exceeded for {IP} on {LimitType}: {Count}/{Limit}", 
                ip, limitType, entry.Count, limit);
            
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = WindowSeconds.ToString();
            await context.Response.WriteAsync($"Rate limit exceeded. Try again in {WindowSeconds} seconds.");
            return;
        }
        
        // Add rate limit headers
        context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = (limit - entry.Count).ToString();
        
        await _next(context);
    }
}

/// <summary>
/// Extension method for adding RateLimitingMiddleware
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}

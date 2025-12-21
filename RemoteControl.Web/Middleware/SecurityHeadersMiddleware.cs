namespace RemoteControl.Web.Middleware;

/// <summary>
/// Middleware to add security headers to all responses
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Prevent MIME type sniffing
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        
        // Prevent clickjacking
        context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
        
        // XSS Protection (legacy but still useful)
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        
        // Referrer policy
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        
        // Permissions policy (disable unused features)
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
        
        // Content Security Policy - allow self and inline styles/scripts for Blazor
        context.Response.Headers["Content-Security-Policy"] = 
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: blob:; " +
            "connect-src 'self' ws: wss:; " +
            "font-src 'self'; " +
            "frame-ancestors 'self';";

        await _next(context);
    }
}

/// <summary>
/// Extension method for adding SecurityHeadersMiddleware
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}

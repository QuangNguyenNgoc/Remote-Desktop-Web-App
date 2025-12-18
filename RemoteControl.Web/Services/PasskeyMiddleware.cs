using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RemoteControl.Web.Services
{
    public class PasskeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PasskeyMiddleware> _logger;
        private readonly string _expectedPasskey;
        private readonly string _cookieName;

        public PasskeyMiddleware(RequestDelegate next, IConfiguration config, ILogger<PasskeyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _expectedPasskey = config["Passkey:Value"] ?? "";
            _cookieName = config["Passkey:CookieName"] ?? "X-Passkey";
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";

            // Public paths
            if (IsPublicPath(path))
            {
                await _next(context);
                return;
            }

            // Protect only /devices* and /remotehub*
            var protectDevices = path.StartsWith("/devices", StringComparison.OrdinalIgnoreCase);
            var protectHub = path.StartsWith("/remotehub", StringComparison.OrdinalIgnoreCase);

            if (!protectDevices && !protectHub)
            {
                await _next(context);
                return;
            }

            // If server hasn't configured passkey, do not block (lab)
            if (string.IsNullOrWhiteSpace(_expectedPasskey))
            {
                _logger.LogWarning("PasskeyMiddleware: Passkey not configured. Protection disabled.");
                await _next(context);
                return;
            }

            if (!IsAuthenticated(context))
            {
                if (protectDevices)
                {
                    var returnUrl = context.Request.Path + context.Request.QueryString;
                    var redirect = $"/login?returnUrl={Uri.EscapeDataString(returnUrl)}";
                    context.Response.Redirect(redirect);
                    return;
                }

                // Hub: return 401
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            await _next(context);
        }

        private bool IsAuthenticated(HttpContext context)
        {
            // Header X-Passkey
            var header = context.Request.Headers["X-Passkey"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(header) &&
                string.Equals(header, _expectedPasskey, StringComparison.Ordinal))
            {
                return true;
            }

            // Cookie X-Passkey (or configured cookie name)
            if (context.Request.Cookies.TryGetValue(_cookieName, out var cookieVal) &&
                !string.IsNullOrWhiteSpace(cookieVal) &&
                string.Equals(cookieVal, _expectedPasskey, StringComparison.Ordinal))
            {
                return true;
            }

            return false;
        }

        private static bool IsPublicPath(string path)
        {
            if (path.Equals("/", StringComparison.OrdinalIgnoreCase)) return true;

            // Login/Auth
            if (path.StartsWith("/login", StringComparison.OrdinalIgnoreCase)) return true;
            if (path.StartsWith("/auth", StringComparison.OrdinalIgnoreCase)) return true;

            // Static / Blazor
            if (path.StartsWith("/css", StringComparison.OrdinalIgnoreCase)) return true;
            if (path.StartsWith("/js", StringComparison.OrdinalIgnoreCase)) return true;
            if (path.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase)) return true;
            if (path.StartsWith("/_content", StringComparison.OrdinalIgnoreCase)) return true;
            if (path.StartsWith("/_blazor", StringComparison.OrdinalIgnoreCase)) return true;
            if (path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase)) return true;

            // Status pages
            if (path.StartsWith("/not-found", StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        }
    }
}

using RemoteControl.Web.Components;
using RemoteControl.Web.Hubs;
using RemoteControl.Web.Services;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// SignalR với MaxMessageSize lớn cho Screenshot base64
builder.Services.AddSignalR(o =>
{
    o.MaximumReceiveMessageSize = 20 * 1024 * 1024; // 20MB
});

// UDP Discovery Broadcaster - cho Agent tự động tìm Server
builder.Services.AddHostedService<DiscoveryBroadcaster>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// ===== Passkey Middleware (protect /devices* and /remotehub*) =====
app.UseMiddleware<PasskeyMiddleware>();

app.UseAntiforgery();

// ===== Auth endpoints (simple) =====
app.MapPost("/auth/login", (HttpContext ctx, PasskeyLoginRequest body, IConfiguration config) =>
{
    var expected = config["Passkey:Value"] ?? "";
    var cookieName = config["Passkey:CookieName"] ?? "X-Passkey";

    if (string.IsNullOrWhiteSpace(expected))
        return Results.Problem("Passkey is not configured on server.");

    var incoming = body?.Passkey ?? "";
    if (!string.Equals(incoming, expected, StringComparison.Ordinal))
        return Results.Unauthorized();

    ctx.Response.Cookies.Append(cookieName, incoming, new CookieOptions
    {
        HttpOnly = true,
        Secure = ctx.Request.IsHttps,
        SameSite = SameSiteMode.Lax,
        Path = "/"
    });

    return Results.Ok();
}).DisableAntiforgery();

app.MapPost("/auth/logout", (HttpContext ctx, IConfiguration config) =>
{
    var cookieName = config["Passkey:CookieName"] ?? "X-Passkey";
    ctx.Response.Cookies.Delete(cookieName, new CookieOptions { Path = "/" });
    return Results.Ok();
}).DisableAntiforgery();

app.MapGet("/auth/me", (HttpContext ctx, IConfiguration config) =>
{
    var expected = config["Passkey:Value"] ?? "";
    var cookieName = config["Passkey:CookieName"] ?? "X-Passkey";

    if (string.IsNullOrWhiteSpace(expected))
        return Results.Ok(new { authenticated = true });

    var header = ctx.Request.Headers["X-Passkey"].FirstOrDefault();
    if (!string.IsNullOrWhiteSpace(header) && string.Equals(header, expected, StringComparison.Ordinal))
        return Results.Ok(new { authenticated = true });

    if (ctx.Request.Cookies.TryGetValue(cookieName, out var cookieVal) &&
        !string.IsNullOrWhiteSpace(cookieVal) &&
        string.Equals(cookieVal, expected, StringComparison.Ordinal))
        return Results.Ok(new { authenticated = true });

    return Results.Unauthorized();
});

// Static + Blazor
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map SignalR Hub
app.MapHub<RemoteControlHub>("/remotehub");

app.Run();

public sealed class PasskeyLoginRequest
{
    public string Passkey { get; set; } = "";
}

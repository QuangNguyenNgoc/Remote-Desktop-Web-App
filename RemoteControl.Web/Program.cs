using RemoteControl.Web.Components;
using RemoteControl.Web.Hubs;
using RemoteControl.Web.Services;

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

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map SignalR Hub
app.MapHub<RemoteControlHub>("/remotehub");

app.Run();

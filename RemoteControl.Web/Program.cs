using RemoteControl.Web.Components;
using RemoteControl.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// IMPORTANT: Screenshot base64 rất lớn -> tăng giới hạn nhận message của SignalR
builder.Services.AddSignalR(o =>
{
    o.MaximumReceiveMessageSize = 20 * 1024 * 1024; // 20MB
});

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

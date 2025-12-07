using RemoteControl.Web.Components;
using RemoteControl.Web.Hubs;            // dùng RemoteControlHub cho SignalR

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR(); // Đăng ký dịch vụ SignalR cho web
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
// Map SignalR Hub cho remote control
// Endpoint: /remotehub - Agent và Dashboard sẽ connect vào đây
app.MapHub<RemoteControlHub>("/remotehub");

app.Run();

# Chi Tiết Tech Stack - Remote Control Desktop

## Tổng Quan C# Ecosystem

### Tại Sao Dùng C# Cho Toàn Bộ Stack?

**Unified Development**:
- 1 ngôn ngữ cho Frontend (Blazor), Backend (ASP.NET Core), Agent (Console App)
- Share code dễ dàng qua Shared library
- Kiến thức tái sử dụng cao
- Tooling nhất quán (Visual Studio, VS Code)

**Performance**:
- .NET 10.0 với ahead-of-time (AOT) compilation
- Memory-efficient với span, stackalloc
- Native interop cho system-level operations (screenshot, keylog)

**Ecosystem**:
- NuGet package manager
- Blazor cho modern web UI
- SignalR cho real-time communication
- Cross-platform deployment (Windows, Linux, macOS - dù hiện tại target Windows)

---

## .NET Framework

### Phiên Bản
-  **Current**: .NET 10.0
- **Target**: `net10.0` (trong `.csproj` files)
- **Release**: Preview (cutting-edge features)

### Tại Sao .NET 10?
- Latest preview với performance improvements
- Native AOT support
- Enhanced Blazor features
- Better SignalR scalability

### .csproj Configuration

```xml
<!-- RemoteControl.Web.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>RemoteControl.Web</RootNamespace>
    <AssemblyName>RemoteControl.Web</AssemblyName>
    <BlazorDisableThrowNavigationException>true</BlazorDisableThrowNavigationException>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\RemoteControl.Shared\RemoteControl.Shared.csproj" />
  </ItemGroup>
</Project>
```

**Key Settings**:
- `Nullable>enable`: Nullable reference types cho type safety
- `ImplicitUsings`: Auto-import common namespaces (System, System.Linq, etc.)
- `BlazorDisableThrowNavigationException`: Disable exception khi navigate

---

## Frontend: Blazor Server

### Blazor Server vs Blazor WebAssembly

| Feature | Blazor Server (✅ Chosen) | Blazor WebAssembly |
|---------|---------------------------|-------------------|
| Execution | Server-side | Client-side (browser) |
| Download Size | Small (~100KB) | Large (~2MB+) |
| First Load | Fast | Slow (download .NET runtime) |
| Offline Support |❌ No | ✅ Yes |
| Server Load | High | Low |
| Real-time | Native (SignalR) | Need extra setup |
| .NET APIs | Full access | Limited (browser sandbox) |

**Lý Do Chọn Server**:
- Real-time là core feature → SignalR đã có sẵn
- Full .NET API access (filesystem, processes)
- Faster initial load
- Easier deployment (no WASM complexity)

### Render Modes

```csharp
// Program.cs
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();  // ← Interactive Server Mode

app.MapRazorComponents<App>()
    .AddInteractive ServerRenderMode();
```

**Interactive Server Mode**:
- Component state trên server
- UI updates qua SignalR
- Events (onClick, onChange) gửi qua SignalR về server

### Blazor Component Structure

```razor
@* Components/Pages/Home.razor *@
@page "/"

@* Markup Section *@
<div class="grid grid-cols-4 gap-6">
    <StatCard Title="Active Agents" Value="12" />
    <StatCard Title="Keylogs" Value="8.4k" />
</div>

@* Code Section *@
@code {
    private List<AgentActivityModel> Activities { get; set; } = new();

    protected override void OnInitialized()
    {
        // Load data
        Activities = GetActivities();
    }

    public record AgentActivityModel
    {
        public string AgentId { get; init; } = "";
        public string EventType { get; init; } = "";
    }
}
```

**Key Concepts**:
- `@page "/path"`: Routing directive
- `@code {}`: C# logic block
- `@inject`: Dependency injection
- Event Handlers: `@onclick="MethodName"`
- Two-way Binding: `@bind="PropertyName"`

### Component Lifecycle

```
OnInitialized / OnInitializedAsync
          ↓
OnParametersSet / OnParametersSetAsync
          ↓
    OnAfterRender / OnAfterRenderAsync
          ↓
      (Component Updates)
          ↓
         Dispose
```

### JavaScript Interop (Planned)

```csharp
// Gọi JS từ C#
await JSRuntime.InvokeVoidAsync("console.log", "Hello from Blazor!");

// Gọi C# từ JS (via JS module)
[JSInvokable]
public static Task<string> GetAgentData(string agentId) { ... }
```

---

## Backend: ASP.NET Core

### Hosting Model

**Kestrel Web Server**:
- Cross-platform, high-performance
- Default cho ASP.NET Core
- Listening ports: HTTP 5048, HTTPS 7102 (dev)

**Configuration**:
```json
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Middleware Pipeline

```csharp
// Program.cs - Thứ tự QUAN TRỌNG!

var app = builder.Build();

// 1. Exception Handling (phải đầu tiên)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");  // Global error handler
    app.UseHsts();                      // HTTP Strict Transport Security
}

// 2. Status Code Pages
app.UseStatusCodePagesWithReExecute("/not-found");  // 404 redirect

// 3. Security
app.UseHttpsRedirection();   // HTTP → HTTPS redirect
app.UseAntiforgery();        // CSRF protection

// 4. Static Files
app.MapStaticAssets();       // Serve wwwroot/

// 5. Routing
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Future: SignalR Hub
// app.MapHub<RemoteControlHub>("/remotehub");

app.Run();
```

**Giải Thích**:
- Exception handler phải ở đầu để catch mọi lỗi
- HTTPS redirect trước khi xử lý requests
- Static files trước routing
- Hub mapping after components

### Dependency Injection

**Current Services**:
```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
```

**Planned Services**:
```csharp
// SignalR
builder.Services.AddSignalR();

// Scoped services (per connection)
builder.Services.AddScoped<IAgentManager, AgentManagerService>();
builder.Services.AddScoped<ICommandService, CommandService>();

// Singleton services (app lifetime)
builder.Services.AddSingleton<ILoggingService, LoggingService>();
```

---

## SignalR

### Hub Implementation (Planned)

```csharp
using Microsoft.AspNetCore.SignalR;

public class RemoteControlHub : Hub
{
    private static Dictionary<string, AgentInfo> _agents = new();

    public async Task RegisterAgent(AgentInfo info)
    {
        info.ConnectionId = Context.ConnectionId;
        _agents[info.AgentId] = info;
        
        await Clients.All.SendAsync("AgentConnected", info);
    }

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        var agent = _agents.Values.FirstOrDefault(a => 
            a.ConnectionId == Context.ConnectionId);
        
        if (agent != null)
        {
            _agents.Remove(agent.AgentId);
            await Clients.All.SendAsync("AgentDisconnected", agent.AgentId);
        }
    }
}
```

### Client Libraries

**Web (Blazor)**:
- Package: `Microsoft.AspNetCore.SignalR.Client` (built-in)
- Connection:
```csharp
var hub = new HubConnectionBuilder()
    .WithUrl(NavigationManager.ToAbsoluteUri("/remotehub"))
    .Build();

await hub.StartAsync();
```

**Agent (Console)**:
- Package: `Microsoft.AspNetCore.SignalR.Client`
- Version: TBD (same as Web)
- Connection:
```csharp
var hub = new HubConnectionBuilder()
    .WithUrl("http://localhost:5048/remotehub")
    .WithAutomaticReconnect()
    .Build();
```

### Connection Strategy

**Transports** (auto-negotiated):
1. WebSockets (preferred)
2. Server-Sent Events
3. Long Polling (fallback)

**Reconnection**:
- Agent: `WithAutomaticReconnect()` - exponential backoff
- Web:Handled by Blazor's `ReconnectModal.razor`

---

## Tailwind CSS

### Configuration

```javascript
// tailwind.config.js
export default {
  content: [
    "./Components/**/*.{razor,html,cshtml}"
  ]
}
```

**Build Process**:
```json
// package.json
{
  "scripts": {
    "watch": "npx tailwindcss -i ./Styles/app.css -o ./wwwroot/css/app.css --watch"
  },
  "dependencies": {
    "@tailwindcss/cli": "^4.1.17",
    "tailwindcss": "^4.1.17"
  }
}
```

**Usage**:
```razor
<div class="grid grid-cols-4 gap-6 p-8 bg-gray-900 text-white">
    <div class="rounded-xl bg-blue-500/10 p-6">
        <h3 class="text-2xl font-bold">12</h3>
    </div>
</div>
```

### CSS Variables (Custom Theming)

```css
/* app.css */
:root {
  --card-bg: #1e293b;
  --card-border: rgba(255, 255, 255, 0.1);
  --text-primary: #f1f5f9;
  --btn-primary-bg: #3b82f6;
}
```

**Integration**:
```razor
<div style="background-color: var(--card-bg); border: 1px solid var(--card-border);">
    ...
</div>
```

---

## Docker (Planned)

### Web Container

```dockerfile
# docker/Dockerfile.web
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["RemoteControl.Web/RemoteControl.Web.csproj", "RemoteControl.Web/"]
COPY ["RemoteControl.Shared/RemoteControl.Shared.csproj", "RemoteControl.Shared/"]
RUN dotnet restore "RemoteControl.Web/RemoteControl.Web.csproj"
COPY . .
WORKDIR "/src/RemoteControl.Web"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RemoteControl.Web.dll"]
```

### Docker Compose

```yaml
# docker-compose.yml
version: '3.8'
services:
  web:
    build:
      context: .
      dockerfile: docker/Dockerfile.web
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
```

---

## Development Tools

### IDEs

**Visual Studio Community 2022**:
- Workloads:
  - ASP.NET and web development
  - .NET desktop development
- Extensions:
  - ReSharper / CodeMaid (optional)

**VS Code**:
- Extensions:
  - C# Dev Kit
  - Tailwind CSS IntelliSense
  - GitLens

### Build Tools

```powershell
# Restore packages
dotnet restore

# Build
dotnet build

# Run (with hot reload)
dotnet watch

# Publish
dotnet publish -c Release
```

### npm Scripts

```bash
# Tailwind watch mode
npm run watch

# Install dependencies
npm install
```

---

## Performance Optimizations

### Blazor Optimizations

**Static Asset CDN**:
```csharp
app.MapStaticAssets();  // Fingerprinted static files
```

**Lazy Loading**:
```razor
@* Lazy load heavy components *@
<Lazy Component="typeof(HeavyChart)" />
```

**Virtualization** (for large lists):
```razor
<Virtualize Items="@LargeList" Context="item">
    <div>@item.Name</div>
</Virtualize>
```

### SignalR Optimizations

**Message Pack Protocol**:
```csharp
builder.Services.AddSignalR()
    .AddMessagePackProtocol();  // Binary protocol, smaller payloads
```

**Compression**:
```csharp
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});
```

---

## Security

### HTTPS Enforcement

```csharp
app.UseHttpsRedirection();
app.UseHsts();  // Production only
```

### CSRF Protection

```csharp
app.UseAntiforgery();  // Required cho Blazor Server forms
```

### Future: Authentication

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { ... });

app.UseAuthentication();
app.UseAuthorization();
```

---

**Cập Nhật**: 02/12/2024  
**Phiên Bản**: 1.0

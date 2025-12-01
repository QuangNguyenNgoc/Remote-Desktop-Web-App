# Kiến Trúc Hệ Thống - Remote Control Desktop

## Sơ Đồ Kiến Trúc Tổng Thể

```
┌──────────────────────────────────────────────────────────────────────┐
│                         INTERNET / NETWORK                            │
└────────────────────────────────────┬─────────────────────────────────┘
                                     │
                    ┌────────────────┴─────────────────┐
                    │                                  │
         ┌──────────▼──────────┐          ┌───────────▼──────────┐
         │   Admin Browser     │          │  Remote Machine #N   │
         │   (Web Dashboard)   │          │    (Agent Client)    │
         │                     │          │                      │
         │  Blazor Server UI   │          │  Console App (.NET)  │
         └──────────┬──────────┘          └───────────┬──────────┘
                    │                                  │
                    │         SignalR Connection       │
                    │    (WebSocket/Long Polling)      │
                    └─────────────┬──────────────────┘
                                 │
                    ┌────────────▼───────────┐
                    │                        │
                    │  RemoteControl.Web     │
                    │  (ASP.NET Core)        │
                    │                        │
                    │  ┌──────────────────┐  │
                    │  │  SignalR Hub     │  │  ← Real-time orchestration
                    │  │  (Planned)       │  │
                    │  └──────────────────┘  │
                    │                        │
                    │  ┌──────────────────┐  │
                    │  │  Blazor Components│  │  ← UI Rendering
                    │  │  (23 files)      │  │
                    │  └──────────────────┘  │
                    │                        │
                    │  ┌──────────────────┐  │
                    │  │  Services        │  │  ← Business Logic (Planned)
                    │  │  (Planned)       │  │
                    │  └──────────────────┘  │
                    └────────────────────────┘
                                 │
                                 │ References
                                 │
                    ┌────────────▼───────────┐
                    │                        │
                    │ RemoteControl.Shared   │
                    │  (Class Library)       │
                    │                        │
                    │  - AgentInfo           │
                    │  - CommandRequest      │
                    │  - CommandResult       │
                    └────────────────────────┘
```

## Kiến Trúc Ba Tầng (Three-Tier Architecture)

### Tier 1: Presentation Layer (Frontend)

**Vị Trí**: `RemoteControl.Web/Components/`

**Công Nghệ**: Blazor Server với Interactive Server Components

**Trách Nhiệm**:
- Render giao diện người dùng
- Xử lý user interactions (click, input)  
- Hiển thị real-time data từ agents
- Routing và navigation

#### Cấu Trúc Components (23 files)

**Layout Components (6 files)**:
```
Components/Layout/
├── MainLayout.razor             # Root layout với Sidebar + TopBar
├── Sidebar.razor                # Left sidebar navigation
├── TopBar.razor                 # Top navigation bar
├── NavMenu.razor                # Navigation menu items
├── ReconnectModal.razor         # SignalR reconnection UI
├── ReconnectModal.razor.css     # Scoped styles
└── ReconnectModal.razor.js      # JS interop cho reconnect
```

**Page Components (7 files)**:
```
Components/Pages/
├── Home.razor                   # Dashboard tổng quan (stat cards, activity log)
├── DeviceManager.razor          # Quản lý danh sách agents
├── DeviceControl.razor          # Control panel cho 1 agent cụ thể
├── Counter.razor                # Demo component
├── Weather.razor                # Demo component
├── Error.razor                  # Error page
└── NotFound.razor               # 404 page
```

**Shared/Reusable Components (8 files)**:
```
Components/Shared/
├── StatCard.razor               # Card hiển thị metrics (agents, keylogs, screenshots)
├── DeviceCard.razor             # Card hiển thị 1 agent (ID, status, actions)
├── DeviceHeader.razor           # Header cho device detail page
├── RemoteScreen.razor           # Component hiển thị screenshot từ agent
├── ResourceUsageCard.razor      # Card hiển thị CPU/RAM realtime
├── TerminalLog.razor            # Terminal-style log display
├── SearchInput.razor            # Search box với icon
└── NavItem.razor                # Single navigation item
```

**Key Patterns**:
- **RenderFragment**: `StatCard` sử dụng `<Icon>` và `<SubContent>` slots
- **Event Callbacks**: Components emit events lên parent
- **State Management**: Component-level state với `@code` blocks
- **CSS Variables**: Dynamic theming với custom properties

---

### Tier 2: Logic Layer (Backend)

**Vị Trí**: `RemoteControl.Web/Hubs/`, `RemoteControl.Web/Services/`

**Công Nghệ**: ASP.NET Core, SignalR

**Trách Nhiệm**:
- Quản lý kết nối agents qua SignalR
- Route commands từ admin → agents
- Broadcast events từ agents → admin dashboards  
- Validate requests
- Business logic processing

#### Program.cs - Middleware Pipeline

```csharp
// Location: RemoteControl.Web/Program.cs

var builder = WebApplication.CreateBuilder(args);

// Services Registration
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Future: builder.Services.AddSignalR();
// Future: builder.Services.AddScoped<IAgentManager, AgentManagerService>();

var app = builder.Build();

// Middleware Pipeline (thứ tự quan trọng!)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Future: app.MapHub<RemoteControlHub>("/remotehub");

app.Run();
```

**Services (Planned)**:
```
Services/
├── AgentManagerService.cs       # Quản lý dictionary của connected agents
│   - RegisterAgent(AgentInfo)
│   - UnregisterAgent(string agentId)
│   - GetAgentById(string agentId)
│   - GetAllAgents()
│   - UpdateSystemInfo(string agentId, SystemInfo)
│
├── CommandService.cs            # Xử lý command routing
│   - SendCommand(string agentId, CommandRequest)
│   - TrackCommand(string commandId)
│   - GetCommandStatus(string commandId)
│
└── LoggingService.cs            # Logging agent activities
    - LogActivity(string agentId, string eventType)
    - GetRecentActivities(int count)
```

**Hubs (Planned)**:
```
Hubs/
└── RemoteControlHub.cs
    # Client → Hub methods:
    - RegisterAgent(AgentInfo)
    - SendResult(string commandId, CommandResult)
    - UpdateSystemInfo(SystemInfo)
    - Heartbeat(string agentId)
    
    # Hub → Client methods (via Clients.All/Clients.Client()):
    - ExecuteCommand(CommandRequest)
    - AgentConnected(AgentInfo)
    - AgentDisconnected(string agentId)
    - CommandCompleted(CommandResult)
    - SystemInfoUpdated(string agentId, SystemInfo)
```

---

###  Tier 3: Agent Layer (Desktop Client)

**Vị Trí**: `RemoteControl.Agent/`

**Công Nghệ**: C# Console Application, .NET 10.0

**Trách Nhiệm**:
- Kết nối đến SignalR Hub
- Lắng nghe commands từ Hub
- Thực thi các operations (screenshot, process management, keylogger)
- Gửi kết quả về Hub

#### Dự Kiến Cấu Trúc

```
RemoteControl.Agent/
│
├── Program.cs                   # Entry point, SignalR client setup
│   - Main()
│   - ConnectToHub()
│   - RegisterEventHandlers()
│   - HeartbeatLoop()
│
├── Services/
│   ├── ScreenshotService.cs     # Capture màn hình
│   │   - CaptureScreen() → byte[]
│   │   - SaveScreenshot(string path)
│   │
│   ├── ProcessService.cs        # Quản lý processes
│   │   - ListProcesses() → List<ProcessInfo>
│   │   - StartProcess(string name)
│   │   - KillProcess(int pid)
│   │
│   ├── KeylogService.cs         # Keyboard monitoring
│   │   - StartKeylogger()
│   │   - StopKeylogger()
│   │   - GetKeylogData() → List<KeylogEntry>
│   │
│   ├── SystemInfoService.cs     # System metrics
│   │   - GetCpuUsage() → double
│   │   - GetMemoryUsage() → double
│   │   - GetProcessCount() → int
│   │
│   └── WebcamService.cs         # Webcam control (Planned)
│       - TurnOn()
│       - TurnOff()
│       - CaptureImage() → byte[]
│
└── Handlers/
    └── CommandHandler.cs        # Route commands → services
        - HandleCommand(CommandRequest) → CommandResult
        - HandleListProcesses()
        - HandleCaptureScreen()
        - HandleStartKeylogger()
        - etc.
```

**Agent Program.cs (Dự Kiến)**:
```csharp
using Microsoft.AspNetCore.SignalR.Client;
using RemoteControl.Shared.Models;

var hubConnection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5048/remotehub")
    .WithAutomaticReconnect()
    .Build();

// Listen for commands from Hub
hubConnection.On<CommandRequest>("ExecuteCommand", async (command) =>
{
    var handler = new CommandHandler();
    var result = await handler.HandleCommand(command);
    await hubConnection.InvokeAsync("SendResult", result);
});

await hubConnection.StartAsync();
Console.WriteLine("Agent connected to Hub");

// Register agent
var agentInfo = new AgentInfo
{
    AgentId = Guid.NewGuid().ToString(),
    MachineName = Environment.MachineName,
    IpAddress = GetLocalIP(),
    OsVersion = Environment.OSVersion.ToString(),
    Status = AgentStatus.Online
};

await hubConnection.InvokeAsync("RegisterAgent", agentInfo);

// Keep alive
await Task.Delay(-1);
```

---

## Shared Models Layer

**Vị Trí**: `RemoteControl.Shared/Models/`

**Mục Đích**: DTOs (Data Transfer Objects) dùng chung giữa Web và Agent

### AgentInfo.cs

```csharp
public class AgentInfo
{
    public string AgentId { get; set; }           // Unique ID
    public string MachineName { get; set; }       // DESKTOP-ABC123
    public string IpAddress { get; set; }         // 192.168.1.10
    public string OsVersion { get; set; }         // Windows 11
    public DateTime ConnectedAt { get; set; }
    public DateTime LastSeen { get; set; }
    public AgentStatus Status { get; set; }       // Online/Offline/Busy/Error
    public SystemInfo? SystemInfo { get; set; }
}

public class SystemInfo
{
    public double CpuUsage { get; set; }          // 0-100%
    public double MemoryUsage { get; set; }       // 0-100%
    public long TotalMemoryMB { get; set; }       // 16384 MB
    public int ProcessCount { get; set; }         // 142 processes
}

public enum AgentStatus
{
    Online, Offline, Busy, Error
}
```

**Sử Dụng**:
- Agent → Hub: Gửi `AgentInfo` khi connect
- Hub → Web: Broadcast `AgentInfo` cho dashboards
- Web: Hiển thị trong `DeviceCard.razor`, `Home.razor`

### CommandRequest.cs

```csharp
public class CommandRequest
{
    public string CommandId { get; set; }         // Tracking ID
    public string AgentId { get; set; }           // Target agent
    public CommandType Type { get; set; }         // ListProcesses, CaptureScreen, etc.
    public Dictionary<string, string> Parameters { get; set; }
    public DateTime RequestedAt { get; set; }
}

public enum CommandType
{
    // 22 command types total
    ListApplications, KillApplication, StartApplication,
    ListProcesses, KillProcess, StartProcess,
    GetSystemInfo, StartKeylogger, StopKeylogger, GetKeylogData,
    CaptureScreen, Shutdown, Restart,
    WebcamOn, WebcamOff, CaptureWebcam,
    ReadRegistry, WriteRegistry, CreateRegistryKey,
    DeleteRegistryKey, DeleteRegistryValue, Quit
}
```

**Sử Dụng**:
- Web → Hub: Admin gửi command
- Hub → Agent: Route command đến agent cụ thể

### CommandResult.cs

```csharp
public class CommandResult
{
    public string CommandId { get; set; }         // Match với CommandRequest
    public string AgentId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }           // "Đã diệt process" / "Lỗi"
    public object? Data { get; set; }             // Flexible payload
    public DateTime CompletedAt { get; set; }
}

// Specialized result types
public class ProcessListResult { List<ProcessInfo> Processes; }
public class ScreenshotResult { string ImageBase64; int Width; int Height; }
public class KeylogResult { List<KeylogEntry> Entries; }
public class RegistryResult { string KeyPath; string Value; }
```

**Sử Dụng**:
- Agent → Hub: Trả kết quả sau khi execute
- Hub → Web: Broadcast kết quả về admin dashboard

---

## Luồng Giao Tiếp (Communication Flow)

### Example: Screenshot Capture Flow

```
[Admin Dashboard Home.razor]
         │
         │ 1. User clicks "Capture Screen" button (DeviceControl.razor)
         │
         ▼
  [JavaScript/Blazor Event Handler]
         │
         │ 2. Call: hubConnection.InvokeAsync("SendCommand", agentId, CommandType.CaptureScreen)
         │
         ▼
    [RemoteControlHub]
         │
         │ 3. Hub receives command
         │ 4. Tạo CommandRequest { CommandId, AgentId, Type: CaptureScreen }
         │ 5. Get connectionId từ AgentId
         │
         ▼
  [Clients.Client(connectionId).SendAsync("ExecuteCommand", commandRequest)]
         │
         │ 6. SignalR routes command qua WebSocket/LongPolling
         │
         ▼
    [Agent Program.cs]
         │
         │ 7. hubConnection.On<CommandRequest>("ExecuteCommand") fires
         │
         ▼
  [CommandHandler.HandleCommand()]
         │
         │ 8. Switch case: CommandType.CaptureScreen
         │
         ▼
  [ScreenshotService.CaptureScreen()]
         │
         │ 9. Bitmap capture → Convert to Base64
         │
         ▼
  [CommandResult created]
         │
         │ 10. result.Data = new ScreenshotResult { ImageBase64 = "..." }
         │
         ▼
  [hubConnection.InvokeAsync("SendResult", result)]
         │
         │ 11. Send kết quả về Hub
         │
         ▼
    [RemoteControlHub.SendResult()]
         │
         │ 12. Hub receives result
         │ 13. Validate CommandId
         │
         ▼
  [Clients.All.SendAsync("CommandCompleted", result)]
         │
         │ 14. Broadcast kết quả cho mọi admin dashboards
         │
         ▼
  [Dashboard Component]
         │
         │ 15. hubConnection.On<CommandResult>("CommandCompleted")
         │ 16. if (result.Data is ScreenshotResult screenshot) { ... }
         │
         ▼
   [RemoteScreen.razor]
         │
         │ 17. Hiển thị image: <img src="data:image/png;base64,{screenshot.ImageBase64}" />
         │
         ▼
      [User sees screenshot]
```

### SignalR Event Flow

| Event Name | Direction | Sender | Receiver | Payload | Purpose |
|------------|-----------|--------|----------|---------|---------|
| `RegisterAgent` | Agent → Hub | Agent | Hub | `AgentInfo` | Agent đăng ký khi connect |
| `AgentConnected` | Hub → Web | Hub | All Clients | `AgentInfo` | Notify dashboards về agent mới |
| `SendCommand` | Web → Hub | Dashboard | Hub | `agentId, CommandRequest` | Admin gửi lệnh |
| `ExecuteCommand` | Hub → Agent | Hub | Specific Agent | `CommandRequest` | Hub route lệnh đến agent |
| `SendResult` | Agent → Hub | Agent | Hub | `CommandResult` | Agent trả kết quả |
| `CommandCompleted` | Hub → Web | Hub | All Clients | `CommandResult` | Hub broadcast kết quả |
| `UpdateSystemInfo` | Agent → Hub | Agent | Hub | `SystemInfo` | Agent update metrics (mỗi 5s) |
| `SystemInfoUpdated` | Hub → Web | Hub | All Clients | `agentId, SystemInfo` | Hub broadcast metrics update |
| `AgentDisconnected` | Hub → Web | Hub | All Clients | `agentId` | Notify khi agent offline |

---

## Deployment Architecture (Planned)

```
┌────────────────────────────────────────────────────────┐
│                    Production Server                    │
│  ┌──────────────────────────────────────────────────┐  │
│  │          Docker Container: Web                   │  │
│  │  ┌────────────────────────────┐                  │  │
│  │  │  ASP.NET Core (Kestrel)    │                  │  │
│  │  │  Port: 5000 -> 80          │                  │  │
│  │  └────────────────────────────┘                  │  │
│  │  ┌────────────────────────────┐                  │  │
│  │  │  SignalR Hub               │                  │  │
│  │  │  /remotehub endpoint       │                  │  │
│  │  └────────────────────────────┘                  │  │
│  └──────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────┘
                          ▲
                          │ Internet (Port 443 HTTPS)
                          │
           ┌──────────────┼──────────────┐
           │              │              │
    ┌──────▼─────┐ ┌─────▼──────┐ ┌────▼───────┐
    │ Agent #1   │ │ Agent #2   │ │ Agent #N   │
    │ (Desktop)  │ │ (Desktop)  │ │ (Desktop)  │
    └────────────┘ └────────────┘ └────────────┘
```

---

## Security Architecture (Planned)

### Authentication & Authorization

```
┌─────────────┐
│   Browser   │
└──────┬──────┘
       │ 1. Login with credentials
       │
       ▼
┌─────────────────┐
│ Authentication  │  → JWT Token issuance
│   Service       │
└──────┬──────────┘
       │ 2. Token stored in browser
       │
       ▼
┌─────────────────┐
│  SignalR Hub    │  → Validate token on connection
│  Authorization  │
└──────┬──────────┘
       │ 3. Authorize commands (Role-based)
       │
       ▼
┌─────────────────┐
│  Agent Auth     │  → Agent API key verification
│   Verification  │
└─────────────────┘
```

**Planned Security Measures**:
- JWT Authentication cho Admin users
- API Key authentication cho Agents
- Role-based authorization (Admin/ Viewer/Operator)
- Command whitelisting
- Rate limiting
- HTTPS only in production

---

## Scalability Considerations

### Current: Single Server
- 1 Web server instance
- All agents connect to same Hub

### Future: Load Balanced
```
        ┌─────────────┐
        │ Load Balancer│
        └──────┬──────┘
               │
    ┌──────────┼───────────┐
    │          │           │
┌───▼────┐ ┌──▼─────┐ ┌──▼─────┐
│ Web #1 │ │ Web #2 │ │ Web #3 │
└───┬────┘ └───┬────┘ └───┬────┘
    │          │          │
    └──────────┼──────────┘
               │
        ┌──────▼──────┐
        │  Redis      │  ← SignalR backplane
        │  (PubSub)   │
        └─────────────┘
```

**Challenges**:
- Sticky sessions cho SignalR
- Redis backplane cho multi-server
- Database cho persistent storage (nếu cần)

---

**Cập Nhật**: 02/12/2024  
**Phiên Bản**: 1.0

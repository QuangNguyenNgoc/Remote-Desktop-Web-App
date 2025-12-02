# SignalR Communication Protocol - Remote Control Desktop

## Hub Overview

### Hub URL & Configuration

**Development**:
- URL: `/remotehub`
- Full path: `http://localhost:5048/remotehub` (HTTP) hoặc `https://localhost:7102/remotehub` (HTTPS)

**Production** (Planned):
- URL: `https://your-domain.com/remotehub` 
- With authentication & HTTPS enforced

### Hub Class Location

**File**: `RemoteControl.Web/Hubs/RemoteControlHub.cs` (Planned)

**Namespace**: `RemoteControl.Web.Hubs`

---

## SignalR Event Definitions

### Agent → Hub Events

#### 1. RegisterAgent

**Mô Tả**: Agent đăng ký khi kết nối lần đầu

**Trigger**: Agent startup sau khi establish connection

**Payload**:
```csharp
AgentInfo {
    AgentId: "a1b2c3-...",
    MachineName: "DESKTOP-ABC",
    IpAddress: "192.168.1.100",
    OsVersion: "Windows 11",
    ConnectedAt: DateTime.UtcNow,
    Status: AgentStatus.Online,
    SystemInfo: { ... }
}
```

**Hub Handler** (Planned):
```csharp
public async Task RegisterAgent(AgentInfo info)
{
    info.ConnectionId = Context.ConnectionId;
    _connectedAgents[info.AgentId] = info;
    
    await Clients.All.SendAsync("AgentConnected", info);
    
    _logger.LogInformation("Agent {AgentId} registered from {IP}", 
        info.AgentId, info.IpAddress);
}
```

**Response**: Broadcast "AgentConnected" to all dashboards

---

#### 2. SendResult

**Mô Tả**: Agent gửi kết quả sau khi thực thi command

**Trigger**: Command execution completed

**Payload**:
```csharp
CommandResult {
    CommandId: "cmd-uuid-123",  // Matches CommandRequest.CommandId
    AgentId: "agent-abc",
    Success: true,
    Message: "Đã diệt process",
    Data: { ... },  // ProcessListResult, ScreenshotResult, etc.
    CompletedAt: DateTime.UtcNow
}
```

**Hub Handler** (Planned):
```csharp
public async Task SendResult(CommandResult result)
{
    // Notify all dashboards
    await Clients.All.SendAsync("CommandCompleted", result);
    
    // Remove from pending commands
    _pendingCommands.TryRemove(result.CommandId, out _);
}
```

---

#### 3. UpdateSystemInfo

**Mô Tả**: Agent gửi system metrics định kỳ (mỗi 5s)

**Trigger**: Timer/loop trong Agent

**Payload**:
```csharp
SystemInfo {
    CpuUsage: 45.2,
    MemoryUsage: 78.5,
    TotalMemoryMB: 16384,
    ProcessCount: 142
}
```

**Hub Handler** (Planned):
```csharp
public async Task UpdateSystemInfo(SystemInfo systemInfo)
{
    var agentId = GetAgentIdFromContext();
    
    if (_connectedAgents.TryGetValue(agentId, out var agent))
    {
        agent.SystemInfo = systemInfo;
        agent.LastSeen = DateTime.UtcNow;
        
        await Clients.All.SendAsync("SystemInfoUpdated", agentId, systemInfo);
    }
}
```

---

#### 4. Heartbeat

**Mô Tả**: Keep-alive signal từ Agent

**Trigger**: Mỗi 10 giây

**Payload**: `string agentId`

**Hub Handler** (Planned):
```csharp
public async Task Heartbeat(string agentId)
{
    if (_connectedAgents.TryGetValue(agentId, out var agent))
    {
        agent.LastSeen = DateTime.UtcNow;
        agent.Status = AgentStatus.Online;
    }
}
```

---

### Hub → Agent Events

#### 1. ExecuteCommand

**Mô Tả**: Hub gửi command cho agent cụ thể

**Trigger**: Admin clicks button trong dashboard (e.g., "Kill Process", "Capture Screenshot")

**Payload**:
```csharp
CommandRequest {
    CommandId: "cmd-uuid-456",
    AgentId: "agent-xyz",
    Type: CommandType.CaptureScreen,
    Parameters: {},
    RequestedAt: DateTime.UtcNow
}
```

**Hub Method** (Planned):
```csharp
public async Task SendCommand(CommandRequest command)
{
    var agent = _connectedAgents[command.AgentId];
    await Clients.Client(agent.ConnectionId).SendAsync("ExecuteCommand", command);
    
    _pendingCommands[command.CommandId] = command;
}
```

**Agent Handler** (Planned):
```csharp
// Agent Program.cs
hubConnection.On<CommandRequest>("ExecuteCommand", async (command) =>
{
    var handler = new CommandHandler();
    var result = await handler.HandleCommand(command);
    await hubConnection.InvokeAsync("SendResult", result);
});
```

**Expected Response**: Agent calls `SendResult` với CommandResult

---

#### 2. ForceDisconnect

**Mô Tả**: Hub yêu cầu agent disconnect (admin kicked)

**Trigger**: Admin clicks "Disconnect Agent" button

**Payload**: `string reason`

**Hub Method** (Planned):
```csharp
public async Task KickAgent(string agentId, string reason)
{
    var agent = _connectedAgents[agentId];
    await Clients.Client(agent.ConnectionId).SendAsync("ForceDisconnect", reason);
}
```

**Agent Handler**:
```csharp
hubConnection.On<string>("ForceDisconnect", async (reason) =>
{
    Console.WriteLine($"Disconnected by admin: {reason}");
    await hubConnection.StopAsync();
    Environment.Exit(0);
});
```

---

### Hub → Web (Dashboard) Events

#### 1. AgentConnected

**Mô Tả**: Thông báo agent mới kết nối

**Trigger**: Agent calls `RegisterAgent`

**Payload**: `AgentInfo`

**Blazor Handler**:
```razor
@code {
    private HubConnection? hubConnection;

    protected override async Task OnInitializedAsync()
    {
        hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/remotehub"))
            .Build();

        hubConnection.On<AgentInfo>("AgentConnected", (agent) =>
        {
            Agents.Add(agent);
            StateHasChanged();  // Trigger UI update
            ShowNotification($"Agent {agent.MachineName} connected");
        });

        await hubConnection.StartAsync();
    }
}
```

**UI Update**: Add new DeviceCard to dashboard grid

---

#### 2. AgentDisconnected

**Mô Tả**: Thông báo agent ngắt kết nối

**Trigger**: Agent calls `Disconnect()` hoặc connection lost

**Payload**: `string agentId`

**Blazor Handler**:
```csharp
hubConnection.On<string>("AgentDisconnected", (agentId) =>
{
    var agent = Agents.FirstOrDefault(a => a.AgentId == agentId);
    if (agent != null)
    {
        agent.Status = AgentStatus.Offline;
        StateHasChanged();
    }
});
```

---

#### 3. CommandCompleted

**Mô Tả**: Command execution kết thúc, gửi kết quả

**Payload**: `CommandResult`

**Blazor Handler**:
```csharp
hubConnection.On<CommandResult>("CommandCompleted", (result) =>
{
    if (result.Success)
    {
        // Process specialized result types
        if (result.Data is ScreenshotResult screenshot)
        {
            DisplayScreenshot(screenshot);
        }
        else if (result.Data is ProcessListResult processList)
        {
            UpdateProcessTable(processList);
        }
    }
    else
    {
        ShowError(result.Message);
    }
    
    StateHasChanged();
});
```

---

#### 4. SystemInfoUpdated

**Mô Tả**: System metrics update từ agent

**Payload**: `(string agentId, SystemInfo systemInfo)`

**Blazor Handler**:
```csharp
hubConnection.On<string, SystemInfo>("SystemInfoUpdated", (agentId, systemInfo) =>
{
    var agent = Agents.FirstOrDefault(a => a.AgentId == agentId);
    if (agent != null)
    {
        agent.SystemInfo = systemInfo;
        StateHasChanged();
    }
});
```

---

## Connection Management

### Agent Connection Lifecycle

```
1. Agent Startup
    │
    ▼
2. HubConnection.StartAsync()
    │ - Negotiate transport (WebSocket/LongPolling)
    │ - Establish connection
    │
    ▼
3. OnConnectedAsync (Agent side)
    │
    ▼
4. InvokeAsync("RegisterAgent", agentInfo)
    │
    ▼
5. Hub receives → Stores agent in dictionary
    │
    ▼
6. Hub broadcasts "AgentConnected" to all clients
    │
    ▼
7. Dashboards receive → Update UI
    │
    ▼
8. Agent enters main loop
    │ - Listen for "ExecuteCommand"
    │ - Send periodical "UpdateSystemInfo"
    │ - Send "Heartbeat" every 10s
    │
    ▼
[Agent runs until disconnect]
```

### Web Dashboard Connection Lifecycle

```
1. Blazor Component OnInitialized
    │
    ▼
2. HubConnection.StartAsync()
    │
    ▼
3. Register event handlers
    │ - hubConnection.On("AgentConnected", ...)
    │ - hubConnection.On("CommandCompleted", ...)
    │ - hubConnection.On("SystemInfoUpdated", ...)
    │
    ▼
4. Load initial agent list (InvokeAsync("GetAllAgents"))
    │
    ▼
[Dashboard receives real-time updates]
```

---

## Reconnection Strategy

### Agent Reconnection

```csharp
// Agent Program.cs
var hubConnection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5048/remotehub")
    .WithAutomaticReconnect(new[] 
    { 
        TimeSpan.Zero,          // Retry immediately
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(30)
    })
    .Build();

hubConnection.Reconnecting += error =>
{
    Console.WriteLine($"Reconnecting... Error: {error?.Message}");
    return Task.CompletedTask;
};

hubConnection.Reconnected += connectionId =>
{
    Console.WriteLine($"Reconnected with ID: {connectionId}");
    // Re-register agent
    return hubConnection.InvokeAsync("RegisterAgent", _agentInfo);
};

hubConnection.Closed += async error =>
{
    Console.WriteLine($"Connection closed: {error?.Message}");
    await Task.Delay(5000);
    await hubConnection.StartAsync();  // Manual restart
};
```

### Web Dashboard Reconnection

**Blazor Server** tự động handle reconnection với `ReconnectModal.razor`

```razor
@* Components/Layout/ReconnectModal.razor *@
<div id="reconnect-modal" class="@(IsReconnecting ? "visible" : "hidden")">
    <p>Reconnecting to server...</p>
    <div class="spinner"></div>
</div>
```

**JavaScript**:
```javascript
// ReconnectModal.razor.js
Blazor.reconnect = async () => {
    // Auto-reconnect với exponential backoff
};
```

---

## Error Handling

### Connection Errors

**Network Timeout**:
```csharp
try
{
    await hubConnection.StartAsync();
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
    // Retry with backoff
}
```

**Hub Unavailable**:
```csharp
hubConnection.Closed += async error =>
{
    if (error != null)
    {
        _logger.LogError("Hub unavailable: {Error}", error.Message);
        await Task.Delay(5000);
        await hubConnection.StartAsync();
    }
};
```

### Command Errors

**Agent Offline**:
```csharp
public async Task SendCommand(CommandRequest command)
{
    if (!_connectedAgents.ContainsKey(command.AgentId))
    {
        throw new InvalidOperationException("Agent is offline");
    }
    
    // ... send command
}

// Web-side handling
try
{
    await hubConnection.InvokeAsync("SendCommand", command);
}
catch (InvalidOperationException ex)
{
    ShowError($"Cannot send command: {ex.Message}");
}
```

**Permission Denied** (Future):
```csharp
[Authorize(Roles = "Admin")]
public async Task SendCommand(CommandRequest command)
{
    // Only admins can send commands
}
```

**Execution Failure**:
```csharp
// Agent handles và returns error in CommandResult
var result = new CommandResult
{
    Success = false,
    Message = "Process not found",
    Data = null
};
```

---

## Security (Planned)

### Authentication

**JWT Tokens**:
```csharp
// Hub authorization
[Authorize]
public class RemoteControlHub : Hub
{
    public async Task RegisterAgent(AgentInfo info)
    {
        var userId = Context.User?.Identity?.Name;
        // ... authorize and register
    }
}

// Agent authentication
var hubConnection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5048/remotehub", options =>
    {
        options.AccessTokenProvider = () => Task.FromResult(_jwtToken);
    })
    .Build();
```

### Data Encryption

**Transport Security**:
- Development: HTTP + SignalR encryption
- Production: HTTPS (WSS for WebSocket)

**Payload Encryption** (Optional):
```csharp
// Encrypt sensitive data before sending
var encryptedData = AES.Encrypt(screenshot.ImageBase64);
```

---

## Performance Optimizations

### MessagePack Protocol

```csharp
// Program.cs
builder.Services.AddSignalR()
    .AddMessagePackProtocol();  // Binary protocol, smaller payloads
```

**Binary vs JSON**:
- JSON: `{"agentId":"abc","cpuUsage":45.2}` → 36 bytes
- MessagePack: Binary equivalent → ~20 bytes (44% smaller)

### Compression

```csharp
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

app.UseResponseCompression();
```

### Client Results (Streaming)

```csharp
// For large results like screenshots
public async IAsyncEnumerable<byte[]> StreamScreenshot()
{
    var chunks = SplitImageIntoChunks(screenshot);
    foreach (var chunk in chunks)
    {
        yield return chunk;
        await Task.Delay(10);  // Throttle
    }
}
```

---

## Testing SignalR

### Unit Testing Hub Methods

```csharp
public class RemoteControlHubTests
{
    [Fact]
    public async Task RegisterAgent_ShouldStoreAgent()
    {
        var hub = new RemoteControlHub();
        var agentInfo = new AgentInfo { AgentId = "test123" };
        
        await hub.RegisterAgent(agentInfo);
        
        Assert.Contains("test123", hub._connectedAgents.Keys);
    }
}
```

### Integration Testing

```csharp
// Use HubConnection for end-to-end testing
var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5048/remotehub")
    .Build();

await connection.StartAsync();

var result = await connection.InvokeAsync<CommandResult>("SendCommand", command);
Assert.True(result.Success);
```

---

**Cập Nhật**: 02/12/2024  
**Phiên Bản**: 1.0

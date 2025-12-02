# Data Models & DTOs - Remote Control Desktop

## Tổng Quan Shared Models

**Vị Trí**: `RemoteControl.Shared/Models/`  
**Mục Đích**: Data Transfer Objects (DTOs) dùng chung giữa Web và Agent  
**Format**: Plain C# classes với XML documentation

### Tại Sao Cần Shared Library?

```
RemoteControl.Web ────┐
                       ├──► RemoteControl.Shared ◄──┐
RemoteControl.Agent ──┘                             └─ AgentInfo, CommandRequest, CommandResult
```

**Lợi Ích**:
- ✅ Tránh duplicate code
- ✅ Type safety giữa Web ↔ Agent
- ✅ Single source of truth cho data structures
- ✅ Dễ refactor (sửa 1 nơi, apply toàn bộ)

---

## 1. AgentInfo - Thông Tin Agent

### Full Class Definition

```csharp
// File: RemoteControl.Shared/Models/AgentInfo.cs

namespace RemoteControl.Shared.Models;

public class AgentInfo
{
    // Identification
    public string AgentId { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    
    // Timing
    public DateTime ConnectedAt { get; set; }
    public DateTime LastSeen { get; set; }
    
    // Status
    public AgentStatus Status { get; set; }
    public SystemInfo? SystemInfo { get; set; }
}

public class SystemInfo
{
    public double CpuUsage { get; set; }          // 0-100%
    public double MemoryUsage { get; set; }       // 0-100%
    public long TotalMemoryMB { get; set; }       // e.g., 16384 MB
    public int ProcessCount { get; set; }         // e.g., 142
}

public enum AgentStatus
{
    Online,   // Kết nối và sẵn sàng
    Offline,  // Ngắt kết nối
    Busy,     // Đang thực thi command
    Error     // Lỗi xảy ra
}
```

### Giải Thích Properties

| Property | Type | Description | Example |
|----------|------|-------------|---------|
| `AgentId` | `string` | Unique identifier (UUID) | `"a1b2c3-d4e5-..."` |
| `MachineName` | `string` | Computer name | `"DESKTOP-ABC123"` |
| `IpAddress` | `string` | Local IP | `"192.168.1.100"` |
| `OsVersion` | `string` | OS info | `"Microsoft Windows NT 10.0.19045.0"` |
| `ConnectedAt` | `DateTime` | Thời điểm connect | `2024-12-02 10:30:00 UTC` |
| `LastSeen` | `DateTime` | Lần cuối nhận heartbeat | `2024-12-02 10:35:12 UTC` |
| `Status` | `AgentStatus` | Trạng thái hiện tại | `AgentStatus.Online` |
| `SystemInfo` | `SystemInfo?` | Metrics hệ thống (nullable) | `{ CpuUsage: 45.2, ... }` |

### Sử Dụng

**Agent Side** - Khi connect:
```csharp
var agentInfo = new AgentInfo
{
    AgentId = Guid.NewGuid().ToString(),
    MachineName = Environment.MachineName,
    IpAddress = GetLocalIPAddress(),
    OsVersion = Environment.OSVersion.ToString(),
    ConnectedAt = DateTime.UtcNow,
    LastSeen = DateTime.UtcNow,
    Status = AgentStatus.Online,
    SystemInfo = new SystemInfo
    {
        CpuUsage = GetCpuUsage(),
        MemoryUsage = GetMemoryUsage(),
        TotalMemoryMB = GetTotalMemory(),
        ProcessCount = Process.GetProcesses().Length
    }
};

await hubConnection.InvokeAsync("RegisterAgent", agentInfo);
```

**Web Side** - Hiển thị trong Dashboard:
```razor
@* Components/Pages/Home.razor *@
@foreach (var agent in Agents)
{
    <DeviceCard AgentInfo="@agent" />
}

@* DeviceCard.razor *@
<div class="card">
    <h3>@AgentInfo.MachineName</h3>
    <p>IP: @AgentInfo.IpAddress</p>
    <p>Status: @AgentInfo.Status</p>
    <p>CPU: @AgentInfo.SystemInfo?.CpuUsage.ToString("F1")%</p>
</div>
```

### Data Flow

```
Agent (Program.cs)
    │
    │ 1. Tạo AgentInfo object
    │
    ▼
SignalR Hub (Web)
    │
    │ 2. RegisterAgent(AgentInfo)
    │ 3. Lưu vào Dictionary<string, AgentInfo>
    │
    ▼
Blazor Dashboard (Home.razor)
    │
    │ 4. Hiển thị trong DeviceCard
    │
    ▼
User thấy thông tin agent
```

---

## 2. CommandRequest - Lệnh Điều Khiển

### Full Class Definition

```csharp
// File: RemoteControl.Shared/Models/CommandRequest.cs

namespace RemoteControl.Shared.Models;

public class CommandRequest
{
    public string CommandId { get; set; } = Guid.NewGuid().ToString();
    public string AgentId { get; set; } = string.Empty;
    public CommandType Type { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}

public enum CommandType
{
    // Application Management
    ListApplications, KillApplication, StartApplication,
    
    // Process Management
    ListProcesses, KillProcess, StartProcess,
    
    // System Monitoring
    GetSystemInfo,
    
    // Keylogger
    StartKeylogger, StopKeylogger, GetKeylogData,
    
    // Screenshot
    CaptureScreen,
    
    // System Control
    Shutdown, Restart,
    
    // Webcam (Future)
    WebcamOn, WebcamOff, CaptureWebcam,
    
    // Registry
    ReadRegistry, WriteRegistry, CreateRegistryKey,
    DeleteRegistryKey, DeleteRegistryValue,
    
    // Control
    Quit
}
```

### Command Types Reference

| Command | Parameters Required | Purpose | Example Parameters |
|---------|---------------------|---------|-------------------|
| `ListProcesses` | (none) | Lấy danh sách processes | - |
| `KillProcess` | `ProcessId` | Kill process theo ID | `{ "ProcessId": "1234" }` |
| `StartProcess` | `ProcessName` | Khởi động process | `{ "ProcessName": "chrome" }` |
| `CaptureScreen` | (none) | Chụp màn hình | - |
| `StartKeylogger` | (none) | Bật keylogger | - |
| `StopKeylogger` | (none) | Tắt keylogger | - |
| `GetKeylogData` | (none) | Lấy dữ liệu keylog | - |
| `Shutdown` | (none) | Tắt máy | - |
| `WebcamOn` | (none) | Bật webcam | - |
| `ReadRegistry` | `Link`, `ValueName` | Đọc registry | `{ "Link": "HKEY_...", "ValueName": "..." }` |

### Sử Dụng

**Web Side** - Gửi command:
```csharp
// DeviceControl.razor.cs
public async Task KillProcessClicked(int processId)
{
    var command = new CommandRequest
    {
        CommandId = Guid.NewGuid().ToString(),
        AgentId = SelectedAgentId,
        Type = CommandType.KillProcess,
        Parameters = new() { { "ProcessId", processId.ToString() } },
        RequestedAt = DateTime.UtcNow
    };

    await hubConnection.InvokeAsync("SendCommand", command);
}
```

**Hub Side** - Route command:
```csharp
// RemoteControlHub.cs  
public async Task SendCommand(CommandRequest command)
{
    // Get agent connection ID
    var agentConnectionId = _agents[command.AgentId].ConnectionId;
    
    // Send to specific agent
    await Clients.Client(agentConnectionId).SendAsync("ExecuteCommand", command);
}
```

**Agent Side** - Nhận & execute:
```csharp
// Agent Program.cs
hubConnection.On<CommandRequest>("ExecuteCommand", async (command) =>
{
    var result = command.Type switch
    {
        CommandType.ListProcesses => await HandleListProcesses(),
        CommandType.KillProcess => await HandleKillProcess(command.Parameters["ProcessId"]),
        CommandType.CaptureScreen => await HandleCaptureScreen(),
        _ => new CommandResult { Success = false, Message = "Unknown command" }
    };
    
    result.CommandId = command.CommandId;
    result.AgentId = command.AgentId;
    
    await hubConnection.InvokeAsync("SendResult", result);
});
```

---

## 3. CommandResult - Kết Quả Thực Thi

### Full Class Definition

```csharp
// File: RemoteControl.Shared/Models/CommandResult.cs

namespace RemoteControl.Shared.Models;

public class CommandResult
{
    public string CommandId { get; set; } = string.Empty;  // Match CommandRequest.CommandId
    public string AgentId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;   // "Đã diệt process" / "Lỗi"
    public object? Data { get; set; }                      // Flexible payload
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}
```

### Specialized Result Types

#### ProcessListResult

```csharp
public class ProcessListResult
{
    public List<ProcessInfo> Processes { get; set; } = new();
}

public class ProcessInfo
{
    public int ProcessId { get; set; }              // 1234
    public string ProcessName { get; set; } = "";   // "chrome"
    public string? WindowTitle { get; set; }        // "Google Chrome"
    public long MemoryUsageMB { get; set; }         // 250 MB
    public double CpuUsage { get; set; }            // 15.2%
    public int ThreadCount { get; set; }            // 45
}
```

**Sử Dụng**:
```csharp
// Agent
var result = new CommandResult
{
    Success = true,
    Data = new ProcessListResult
    {
        Processes = Process.GetProcesses().Select(p => new ProcessInfo
        {
            ProcessId = p.Id,
            ProcessName = p.ProcessName,
            ThreadCount = p.Threads.Count
        }).ToList()
    }
};

// Web - Display
if (result.Data is ProcessListResult processList)
{
    foreach (var proc in processList.Processes)
    {
        Console.WriteLine($"{proc.ProcessName} (PID: {proc.ProcessId})");
    }
}
```

#### ScreenshotResult

```csharp
public class ScreenshotResult
{
    public string ImageBase64 { get; set; } = "";
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
    public string Format { get; set; } = "bmp";  // "bmp", "png", "jpg"
}
```

**Sử Dụng**:
```csharp
// Agent
var bitmap = CaptureScreen();  // Bitmap object
using var ms = new MemoryStream();
bitmap.Save(ms, ImageFormat.Bmp);
var base64 = Convert.ToBase64String(ms.ToArray());

var result = new CommandResult
{
    Success = true,
    Data = new ScreenshotResult
    {
        ImageBase64 = base64,
        Width = bitmap.Width,
        Height = bitmap.Height
    }
};

// Web - Display
<img src="data:image/@screenshot.Format;base64,@screenshot.ImageBase64" 
     alt="Screenshot"
     style="max-width: 100%;" />
```

#### KeylogResult

```csharp
public class KeylogResult
{
    public List<KeylogEntry> Entries { get; set; } = new();
}

public class KeylogEntry
{
    public DateTime Timestamp { get; set; }
    public string WindowTitle { get; set; } = "";
    public string KeyPressed { get; set; } = "";
}
```

**Sử Dụng**:
```razor
@* Web - Display keylog *@
<table>
    <tr><th>Time</th><th>Window</th><th>Key</th></tr>
    @foreach (var entry in keylog.Entries)
    {
        <tr>
            <td>@entry.Timestamp.ToLocalTime()</td>
            <td>@entry.WindowTitle</td>
            <td>@entry.KeyPressed</td>
        </tr>
    }
</table>
```

#### RegistryResult

```csharp
public class RegistryResult
{
    public string KeyPath { get; set; } = "";
    public string? ValueName { get; set; }
    public string? Value { get; set; }
    public string? ValueType { get; set; }          // "String", "DWORD", etc.
    public string OperationMessage { get; set; } = "";
}
```

---

## Data Flow Diagrams

### Agent Registration Flow

```
Agent Startup
    │
    ▼
Create AgentInfo
  - AgentId = Guid.NewGuid()
  - MachineName = Environment.MachineName
  - IpAddress = GetLocalIP()
  - SystemInfo = GetCurrentMetrics()
    │
    ▼
SignalR Connection Established
    │
    ▼
hubConnection.InvokeAsync("RegisterAgent", agentInfo)
    │
    ▼
HUB: RemoteControlHub.RegisterAgent(AgentInfo info)
  - _agents[info.AgentId] = info
  - Clients.All.SendAsync("AgentConnected", info)
    │
    ▼
ALL DASHBOARDS receive "AgentConnected" event
    │
    ▼
Dashboard updates UI with new agent card
```

### Command Execution Flow

```
Admin clicks "Kill Process 1234"
    │
    ▼
Create CommandRequest
  {
    CommandId: "uuid-1234",
    AgentId: "agent-abc",
    Type: CommandType.KillProcess,
    Parameters: { "ProcessId": "1234" }
  }
    │
    ▼
hubConnection.InvokeAsync("SendCommand", commandRequest)
    │
    ▼
HUB: Routes to specific agent via connectionId
    │
    ▼
AGENT: hubConnection.On("ExecuteCommand") fires
    │
    ▼
CommandHandler.Handle(commandRequest)
  - Parse Parameters["ProcessId"]
  - Kill Process
  - Create CommandResult
    │
    ▼
Create CommandResult
  {
    CommandId: "uuid-1234",  // SAME as request
    Success: true,
    Message: "Đã diệt process",
    CompletedAt: DateTime.UtcNow
  }
    │
    ▼
hubConnection.InvokeAsync("SendResult", commandResult)
    │
    ▼
HUB: Broadcast result to dashboards
    │
    ▼
DASHBOARD: hubConnection.On("CommandCompleted") fires
    │
    ▼
Update UI: Show success notification
```

---

## Serialization Notes

### JSON Serialization

```csharp
// SignalR tự động serialize/deserialize
await connection.InvokeAsync("RegisterAgent", agentInfo);
// ↓ SignalR converts to JSON
// {"agentId":"abc","machineName":"DESKTOP-1",...}
// ↓ SignalR converts back to AgentInfo object
```

### Type Casting for Data Property

```csharp
// CommandResult.Data là object?, cần cast
var result = await GetCommandResult();

if (result.Data is ProcessListResult processList)
{
    // Use processList.Processes
}
else if (result.Data is ScreenshotResult screenshot)
{
    // Use screenshot.ImageBase64
}
```

---

## Best Practices

### 1. Luôn Validate Data

```csharp
public async Task SendCommand(CommandRequest command)
{
    if (string.IsNullOrEmpty(command.AgentId))
        throw new ArgumentException("AgentId is required");
    
    if (!_agents.ContainsKey(command.AgentId))
        throw new InvalidOperationException("Agent not found");
    
    // Proceed with sending
}
```

### 2. Use XML Documentation

```csharp
/// <summary>
/// Represents information about a connected agent/client
/// </summary>
public class AgentInfo
{
    /// <summary>
    /// Unique identifier for the agent
    /// </summary>
    public string AgentId { get; set; } = string.Empty;
}
```

### 3. Nullable Reference Types

```csharp
#nullable enable  // Trong .csproj: <Nullable>enable</Nullable>

public class AgentInfo
{
    public SystemInfo? SystemInfo { get; set; }  // Nullable
    public string MachineName { get; set; } = "";  // Non-nullable, default value
}
```

### 4. Record Types (Optional Alternative)

```csharp
// Thay vì class, có thể dùng record cho immutability
public record AgentInfo
{
    public required string AgentId { get; init; }
    public required string MachineName { get; init; }
}

// Usage
var agent = new AgentInfo
{
    AgentId = "abc",
    MachineName = "DESKTOP-1"
};
```

---

**Cập Nhật**: 02/12/2024  
**Phiên Bản**: 1.0

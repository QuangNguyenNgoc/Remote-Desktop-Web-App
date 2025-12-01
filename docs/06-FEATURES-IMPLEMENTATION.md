# Chi Tiết Implementation Features - Remote Control Desktop

## Feature Matrix

| Feature | UI Component | Backend Service | Agent Service | Status | Priority |
|---------|--------------|-----------------|---------------|--------|----------|
| Dashboard Overview | Home.razor ✅ | - | - | ✅ Done | High |
| Device Cards | DeviceCard.razor ✅ | AgentManager (📋) | - | 🚧 UI Only | High |
| Process Management | DeviceControl.razor ✅ | CommandService (📋) | ProcessService (📋) | 🚧 UI Only | High |
| Screenshot | RemoteScreen.razor ✅ | -  | ScreenshotService (📋) | 🚧 UI Only | High |
| Keylogger | TerminalLog.razor ✅ | - | KeylogService (📋) | 🚧 UI Only | Medium |
| System Metrics | ResourceUsageCard.razor ✅ | - | SystemInfoService (📋) | 🚧 UI Only | High |
| Shutdown/Restart | - | - | SystemControlService​ (📋) | 📋 Planned | Medium |
| Webcam Control | - | - | WebcamService (📋) | 📋 Planned | Low |

**Legend**:
- ✅ Done: Fully implemented
- 🚧 UI Only: UI exists, backend  chưa implement
- 📋 Planned: Not yet started

---

## Feature 1: Dashboard Overview

### Status
- [x] UI Components
- [x] Stat Cards (Active Agents, Keylogs, Screenshots, System Load)
- [x] Recent Activity Table
- [x] Resource Usage Panel
- [ ] Real-time updates (SignalR)

### Implementation Details

#### Frontend `Components/Pages/Home.razor`

```razor
@page "/"

<div class="grid grid-cols-4 gap-6">
    <StatCard Title="Active Agents" Value="12" ColorClass="bg-blue-500/10 text-blue-500">
        <Icon>...</Icon>
    </StatCard>
    <!-- 3 more stat cards -->
</div>

<div class="grid grid-cols-1 xl:grid-cols-3 gap-6">
    <!-- Recent Activity Table -->
    <div class="xl:col-span-2">
        <table>...</table>
    </div>
    
    <!-- Resource Usage Card -->
    <div class="xl:col-span-1">
        <ResourceUsageCard />
    </div>
</div>

@code {
    private List<AgentActivityModel> RecentActivities { get; set; } = new();
    
    protected override void OnInitialized()
    {
        // Mock data for now
        RecentActivities = GetMockActivities();
    }
}
```

**Next Steps**:
1. Connect to SignalR Hub
2. Subscribe to "AgentConnected", "CommandCompleted" events
3. Update `RecentActivities` real-time

---

## Feature 2: Device Management

### Status
- [x] Device Cards UI
- [x] Device Manager Page
- [x] Search & Filter UI
- [ ] SignalR connection management
- [ ] Agent registration backend

### UI Components

**`DeviceManager.razor`**: Grid of DeviceCards
```razor
@page "/device-manager"

<SearchInput Placeholder="Search devices..." @bind-Value="SearchQuery" />

<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
    @foreach (var agent in FilteredAgents)
    {
        <DeviceCard AgentInfo="@agent" OnControlClick="() => NavigateToControl(agent.AgentId)" />
    }
</div>

@code {
    private List<AgentInfo> Agents = new();
    private string Search Query = "";
    
    private IEnumerable<AgentInfo> FilteredAgents =>
        Agents.Where(a => a.MachineName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
}
```

**`DeviceCard.razor`**: Single agent display
```razor
<div class="card">
    <h3>@AgentInfo.MachineName</h3>
    <p>IP: @AgentInfo.IpAddress</p>
    <p class="status-badge @GetStatusClass()">@AgentInfo.Status</p>
    
    @if (AgentInfo.SystemInfo != null)
    {
        <p>CPU: @AgentInfo.SystemInfo.CpuUsage%</p>
        <p>RAM: @AgentInfo.SystemInfo.MemoryUsage%</p>
    }
    
    <button @onclick="OnControlClick">Control</button>
</div>

@code {
    [Parameter] public AgentInfo AgentInfo { get; set; }
    [Parameter] public EventCallback OnControlClick { get; set; }
}
```

### Backend Implementation (Planned)

**`Services/AgentManagerService.cs`**:
```csharp
public class AgentManagerService : IAgentManager
{
    private readonly Dictionary<string, AgentInfo> _agents = new();
    
    public void RegisterAgent(AgentInfo agent)
    {
        _agents[agent.AgentId] = agent;
    }
    
    public List<AgentInfo> GetAllAgents() => _agents.Values.ToList();
    
    public AgentInfo? GetAgentById(string id) => 
        _agents.TryGetValue(id, out var agent) ? agent : null;
}
```

---

## Feature 3: Process Management

### Status
- [x] Process table UI
- [x] Kill/Start buttons
- [x] Search & filter
- [ ] Backend command routing
- [ ] Agent process service

### User Flow

```
1. Admin opens DeviceControl.razor
2. Clicks "List Processes" button
   ↓
3. [Future] hubConnection.InvokeAsync("SendCommand", 
       new CommandRequest { Type = CommandType.ListProcesses })
   ↓
4. [Future] Agent receives "ExecuteCommand"
   ↓
5. [Future] Agent calls ProcessService.ListProcesses()
   ↓
6. [Future] Agent sends CommandResult with ProcessListResult
   ↓
7. UI displays processes in table
8. Admin clicks "Kill" on process 1234
   ↓
9. Send CommandRequest { Type = CommandType.KillProcess, Parameters = { "ProcessId": "1234" } }
   ↓
10. Agent kills process
11. UI shows success notification
```

### Agent Service (Planned)

**`RemoteControl.Agent/Services/ProcessService.cs`**:
```csharp
public class ProcessService
{
    public ProcessListResult ListProcesses()
    {
        var processes = Process.GetProcesses()
            .Select(p => new ProcessInfo
            {
                ProcessId = p.Id,
                ProcessName = p.ProcessName,
                WindowTitle = p.MainWindowTitle,
                MemoryUsageMB = p.WorkingSet64 / 1024 / 1024,
                ThreadCount = p.Threads.Count
            })
            .ToList();
        
        return new ProcessListResult { Processes = processes };
    }
    
    public CommandResult KillProcess(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            process.Kill();
            return new CommandResult { Success = true, Message = "Đã diệt process" };
        }
        catch
        {
            return new CommandResult { Success = false, Message = "Lỗi: Process not found" };
        }
    }
}
```

---

## Feature 4: Screenshot Capture

### Status
- [x] RemoteScreen.razor component
- [x] Display base64 image
- [ ] Capture logic in Agent
- [ ] Command routing

### Implementation

**UI Component**:
```razor
<div class="screenshot-viewer">
    @if (ScreenshotData != null)
    {
        <img src="data:image/@ScreenshotData.Format;base64,@ScreenshotData. ImageBase64" 
             alt="Screenshot"
             style="max-width: 100%; border-radius: 8px;" />
        <p>Captured at: @ScreenshotData.CapturedAt.ToLocalTime()</p>
    }
    else
    {
        <button @onclick="CaptureScreenshot">Capture Screenshot</button>
    }
</div>

@code {
    private ScreenshotResult? ScreenshotData;
    
    private async Task CaptureScreenshot()
    {
        // Future: Send command to agent
    }
}
```

**Agent Service (Planned)**:
```csharp
public class ScreenshotService
{
    public ScreenshotResult CaptureScreen()
    {
        var bounds = Screen.PrimaryScreen.Bounds;
        var bitmap = new Bitmap(bounds.Width, bounds.Height);
        var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
        
        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Bmp);
        var base64 = Convert.ToBase64String(ms.ToArray());
        
        return new ScreenshotResult
        {
            ImageBase64 = base64,
            Width = bounds.Width,
            Height = bounds.Height,
            CapturedAt = DateTime.UtcNow,
            Format = "bmp"
        };
    }
}
```

---

## Feature 5: Keylogger

### Status
- [x] TerminalLog.razor component
- [x] Log display UI
- [ ] Keylogger service in Agent
- [ ] Data transmission

### Agent Service (Planned)

```csharp
public class KeylogService
{
    private readonly List<KeylogEntry> _entries = new();
    private bool _isRunning;
    
    public void StartKeylogger()
    {
        _isRunning = true;
        // Hook keyboard events (requires Windows API)
    }
    
    public void StopKeylogger()
    {
        _isRunning = false;
        // Unhook
    }
    
    public KeylogResult GetData()
    {
        return new KeylogResult { Entries = _entries.ToList() };
    }
}
```

---

## Feature 6: System Metrics (Realtime)

### Status
- [x] ResourceUsageCard.razor
- [x] Mock data display
- [ ] Real metrics from Agent
- [ ] Periodic updates (5s interval)

### Agent Implementation (Planned)

```csharp
public class SystemInfoService
{
    public SystemInfo GetCurrentMetrics()
    {
        var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        var ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
        
        return new SystemInfo
        {
            CpuUsage = cpuCounter.NextValue(),
            MemoryUsage = ramCounter.NextValue(),
            TotalMemoryMB = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024 / 1024,
            ProcessCount = Process.GetProcesses().Length
        };
    }
}

// Send every 5 seconds
while (true)
{
    var metrics = systemInfoService.GetCurrentMetrics();
    await hubConnection.InvokeAsync("UpdateSystemInfo", metrics);
    await Task.Delay(5000);
}
```

---

## Implementation Roadmap

### Phase 8 (Current): SignalR Foundation
- [ ] Create RemoteControlHub.cs
- [ ] Implement RegisterAgent, SendCommand, SendResult methods
- [ ] Test Agent-Hub connection
- [ ] Broadcast events to dashboards

### Phase 9: Agent Services
- [ ] ScreenshotService (Priority: High)
- [ ] ProcessService (Priority: High)
- [ ] SystemInfoService (Priority: High)
- [ ] KeylogService (Priority: Medium)
- [ ] SystemControlService - Shutdown/Restart (Priority: Medium)

### Phase 10: Integration
- [ ] Connect Dashboard → Hub → Agent flows
- [ ] Test each feature end-to-end
- [ ] Error handling & retries
- [ ] UI polish (loading states, notifications)

### Phase 11: Advanced Features
- [ ] Webcam control
- [ ] File system browser
- [ ] Registry editor
- [ ] Multi-agent simultaneous control

---

## Testing Checklist

### Per Feature
- [ ] UI renders correctly
- [ ] Command sends to correct agent
- [ ] Agent executes command
- [ ] Result returns to UI
- [ ] Error handling works
- [ ] Loading states display
- [ ] Success/error notifications show

---

**Cập Nhật**: 02/12/2024  
**Phiên Bản**: 1.0

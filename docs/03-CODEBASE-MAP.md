# Bản Đồ Codebase - Remote Control Desktop

## Cấu Trúc Project Tổng Quát

```
RemoteControlProject/
│
├── .git/                                # Git repository
├── .gitignore                           # Git ignore rules
├── .github/                             # GitHub Actions workflows
│
├── RemoteControl.Web/                   # 🌐 Blazor Server Web App
│   ├── Components/ (23 files)
│   ├── wwwroot/
│   ├── Styles/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── package.json
│   └── tailwind.config.js
│
├── RemoteControl.Agent/                 # 🤖 Console App (Agent)
│   ├── Services/ (Planned)
│   ├── Handlers/ (Planned)
│   ├── Program.cs (Minimal)
│   └── README.md
│
├── RemoteControl.Shared/                # 🔗 Class Library
│   ├── Models/
│   │   ├── AgentInfo.cs
│   │   ├── CommandRequest.cs
│   │   └── CommandResult.cs
│   └── README.md
│
├── tests/                               # 🧪 Unit Tests (Planned)
│   ├── RemoteControl.Web.Tests/
│   ├── RemoteControl.Agent.Tests/
│   └── RemoteControl.Integration.Tests/
│
├── docker/                              # 🐳 Docker configs (Planned)
│   └── README.md
│
├── docs/                                # 📚 Documentation
│   ├── 00-PROJECT-OVERVIEW.md
│   ├── 01-ARCHITECTURE.md
│   ├── 02-TECH-STACK-DETAILS.md
│   ├── 03-CODEBASE-MAP.md (this file)
│   ├── 04-DATA-MODELS.md
│   ├── 05-SIGNALR-PROTOCOL.md
│   ├── 06-FEATURES-IMPLEMENTATION.md
│   ├── 07-SETUP-GUIDE.md
│   ├── 08-DEPLOYMENT-GUIDE.md
│   ├── 09-CONTRIBUTING.md
│   ├── backend-guilde/ (folder)
│   └── components-guilde/ (folder)
│
├── RemoteControl.slnx                   # Solution file (XML format)
└── README.md                            # Project README
```

---

## RemoteControl.Web - File Breakdown

### Entry Point

#### `Program.cs` (28 lines)
**Purpose**: Application entry point & configuration

**Key Configurations**:
- Services: `AddRazorComponents().AddInteractiveServerComponents()`
- Middleware pipeline: Exception handler, HTTPS redirect, Antiforgery, Static assets
- Component routing: `MapRazorComponents<App>().AddInteractiveServerRenderMode()`

**Current State**: Basic Blazor Server setup

**Planned Additions**:
```csharp
// Future
builder.Services.AddSignalR();
app.MapHub<RemoteControlHub>("/remotehub");
```

**Lines of Code**: 28

---

#### `appsettings.json` (~15 lines)
**Purpose**: Application configuration

**Current Settings**:
```json
{
  "Logging": { ... },
  "AllowedHosts": "*"
}
```

**Planned Settings**: SignalR hub URL, database connection, etc.

---

### Components - Layout (6 files)

#### `Components/Layout/MainLayout.razor` (600 bytes)
**Purpose**: Root layout component

**Structure**:
```razor
<div class="layout-container">
    <Sidebar />
    <div class="main-content">
        <TopBar />
        <div class="page-content">
            @Body
        </div>
    </div>
</div>
```

**Routes Using This**: All pages (default layout)

---

#### `Components/Layout/Sidebar.razor` (1589 bytes)
**Purpose**: Left sidebar navigation

**Features**:
- Logo/branding
- Navigation menu (`<NavMenu />`)
- Collapse/expand on mobile

**Dependencies**: `NavMenu.razor`

---

#### `Components/Layout/TopBar.razor` (996 bytes)
**Purpose**: Top navigation bar

**Features**:
- Page title
- User profile (future)
- Notifications (future)
- Mobile menu toggle

---

#### `Components/Layout/NavMenu.razor` (1510 bytes)
**Purpose**: Navigation menu items

**Menu Items**:
- Dashboard (`/`)
- Device Manager (`/device-manager`)
- Settings (planned)

**Components Used**: `NavItem.razor`

---

#### `Components/Layout/ReconnectModal.razor` (1320 bytes)
**Purpose**: SignalR reconnection UI

**Files**:
- `ReconnectModal.razor` - Blazor component
- `ReconnectModal.razor.css` (3978 bytes) - Scoped styles
- `ReconnectModal.razor.js` (2364 bytes) - JavaScript interop

**Functionality**: Shows modal when Blazor Server connection lost, handles reconnection

---

### Components - Pages (7 files)

#### `Components/Pages/Home.razor` (162 lines, 7071 bytes)
**Route**: `/`

**Purpose**: Dashboard overview page

**Features**:
- 4x `<StatCard>` (Active Agents, Keylogs, Screenshots, System Load)
- Recent Activity table (mock data)
- `<ResourceUsageCard />` component

**Data**:
```csharp
private List<AgentActivityModel> RecentActivities { get; set; } = new();

public record AgentActivityModel
{
    public string AgentId { get; init; }
    public string EventType { get; init; }
    public string Details { get; init; }
    public string TimeAgo { get; init; }
}
```

**Next Steps**: Connect to real-time SignalR events

---

#### `Components/Pages/DeviceManager.razor`
**Route**: `/device-manager`

**Purpose**: List all connected agents

**Features**:
- Grid of `<DeviceCard>`
- Search/filter agents
- "Control" button → navigate to `/device-control/{agentId}`

**Dependencies**: `DeviceCard.razor`, `SearchInput.razor`

---

#### `Components/Pages/DeviceControl.razor`
**Route**: `/device-control/{agentId?}`

**Purpose**: Control panel for single agent

**Features**:
- `<DeviceHeader>` with agent info
- Process management table
- Screenshot viewer
- Command buttons (Kill, Start, Screenshot, etc.)

**Dependencies**: `DeviceHeader.razor`, `RemoteScreen.razor`

---

#### `Components/Pages/Counter.razor` & `Weather.razor`
**Purpose**: Blazor demo/template pages

**Status**: Can be removed (not used in production)

---

#### `Components/Pages/Error.razor` & `NotFound.razor`
**Purpose**: Error & 404 pages

**Routes**: `/Error`, `/not-found`

---

### Components - Shared (8 files)

#### `Components/Shared/StatCard.razor`
**Purpose**: Reusable stat card component

**Usage**:
```razor
<StatCard Title="Active Agents" Value="12" ColorClass="bg-blue-500/10">
    <Icon>
        <svg>...</svg>
    </Icon>
    <SubContent>
        <p class="text-green-400">+2 connected</p>
    </SubContent>
</StatCard>
```

**RenderFragments**: `Icon`, `SubContent`

---

#### `Components/Shared/DeviceCard.razor`
**Purpose**: Single agent display card

**Parameters**:
```csharp
[Parameter] public AgentInfo AgentInfo { get; set; }
[Parameter] public EventCallback OnControlClick { get; set; }
```

**Features**:
- Agent name, IP, status badge
- CPU/RAM metrics (if available)
- "Control" button

---

#### `Components/Shared/DeviceHeader.razor`
**Purpose**: Header for device control page

**Displays**:
- Machine name
- IP address
- OS version
- Connection status
- Last seen timestamp

---

#### `Components/Shared/RemoteScreen.razor`
**Purpose**: Screenshot display component

**Features**:
- Display base64 image
- Capture button
- Download button (future)

**Example**:
```razor
<img src="data:image/bmp;base64,@ScreenshotBase64" alt="Screenshot" />
```

---

#### `Components/Shared/ResourceUsageCard.razor` (155 lines)
**Purpose**: Real-time CPU/RAM usage display

**Features**:
- CPU usage chart (mock)
- RAM usage chart (mock)
- Disk usage
- Network stats

**Next Steps**: Connect to real SystemInfo updates from agents

---

#### `Components/Shared/TerminalLog.razor`
**Purpose**: Terminal-style log display

**Features**:
- Monospace font
- Scrollable log entries
- Auto-scroll to bottom

**Use Case**: Display keylogger output, command results

---

#### `Components/Shared/SearchInput.razor`
**Purpose**: Reusable search input with icon

**Parameters**:
```csharp
[Parameter] public string Placeholder { get; set; }
[Parameter] public string Value { get; set; }
[Parameter] public EventCallback<string> ValueChanged { get; set; }
```

---

#### `Components/Shared/NavItem.razor`
**Purpose**: Single navigation item

**Usage**:
```razor
<NavItem Href="/" IconPath="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6">
    Dashboard
</NavItem>
```

---

### wwwroot/ - Static Files

```
wwwroot/
├── css/
│   └── app.css               # Compiled Tailwind CSS
├── favicon.png
└── (future: images, fonts, etc.)
```

**app.css**: Generated by Tailwind CLI from `Styles/app.css`

---

### Styles/ - Tailwind Source

```
Styles/
└── app.css                   # Tailwind source (@tailwind base, etc.)
```

**Build Process**: `npm run watch` → compiles to `wwwroot/css/app.css`

---

### Configuration Files

#### `package.json` (451 bytes)
**Purpose**: npm dependencies

**Dependencies**:
```json
{
  "dependencies": {
    "@tailwindcss/cli": "^4.1.17",
    "tailwindcss": "^4.1.17"
  },
  "scripts": {
    "watch": "npx tailwindcss -i ./Styles/app.css -o ./wwwroot/css/app.css --watch"
  }
}
```

---

#### `tailwind.config.js`
**Purpose**: Tailwind CSS configuration

**Content Paths**:
```javascript
export default {
  content: [
    "./Components/**/*.{razor,html,cshtml}"
  ]
}
```

---

#### `RemoteControl.Web.csproj` (536 bytes)
**Purpose**: Project file

**Key Settings**:
- Target: `net10.0`
- Nullable: enabled
- Implicit usings: enabled
- Project Reference: `../RemoteControl.Shared/RemoteControl.Shared.csproj`

---

## RemoteControl.Agent - File Breakdown

### Current State: Minimal Structure

```
RemoteControl.Agent/
├── Services/                 # Empty folder
│   ├── (Planned: ScreenshotService.cs)
│   ├── (Planned: ProcessService.cs)
│   ├── (Planned: KeylogService.cs)
│   └── (Planned: SystemInfoService.cs)
├── Handlers/                 # Empty folder
│   └── (Planned: CommandHandler.cs)
├── Program.cs                # Empty (to be implemented)
├── RemoteControl.Agent.csproj
└── README.md
```

### Planned Implementation

#### `Program.cs` (Planned ~100 lines)
**Purpose**: Agent entry point, SignalR client setup

**Structure**:
```csharp
using Microsoft.AspNetCore.SignalR.Client;
using RemoteControl.Shared.Models;

var hubConnection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5048/remotehub")
    .WithAutomaticReconnect()
    .Build();

// Register event handlers
hubConnection.On<CommandRequest>("ExecuteCommand", async (command) =>
{
    var handler = new CommandHandler();
    var result = await handler.HandleCommand(command);
    await hubConnection.InvokeAsync("SendResult", result);
});

// Connect
await hubConnection.StartAsync();

// Register agent
var agentInfo = new AgentInfo { ... };
await hubConnection.InvokeAsync("RegisterAgent", agentInfo);

// Keep alive
await Task.Delay(-1);
```

---

## RemoteControl.Shared - File Breakdown

### Models/ (3 files)

#### `Models/AgentInfo.cs` (100 lines, 2255 bytes)
**Purpose**: Agent information & status

**Classes**:
```csharp
public class AgentInfo { ... }      // Main class
public class SystemInfo { ... }     // Nested info
public enum AgentStatus { ... }     // Status enum
```

**Properties**: 8 properties in AgentInfo, 4 in SystemInfo

**XML Documentation**: Full coverage

---

#### `Models/CommandRequest.cs` (170 lines, 4258 bytes)
**Purpose**: Command from Web to Agent

**Classes**:
```csharp
public class CommandRequest { ...  }   // Main class
public enum CommandType { ... }        // 22 command types
```

**Command Types**: 22 types across 6 categories (Application, Process, Monitoring, Keylogger, Screenshot, System Control, Webcam, Registry)

---

#### `Models/CommandResult.cs` (182 lines, 4713 bytes)
**Purpose**: Result from Agent to Web

**Classes**:
```csharp
public class CommandResult { ... }      // Main result
public class ProcessListResult { ... }  // Specialized
public class ProcessInfo { ... }
public class ScreenshotResult { ... }
public class KeylogResult { ... }
public class KeylogEntry { ... }
public class RegistryResult { ... }
```

**Total**: 7 classes for flexible result handling

---

## Dependencies Graph

### NuGet Packages

#### RemoteControl.Web
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <ItemGroup>
    <ProjectReference Include="..\RemoteControl.Shared\RemoteControl.Shared.csproj" />
    <!-- Implicit packages from Microsoft.NET.Sdk.Web:
         - Microsoft.AspNetCore.Components.Web
         - Microsoft.AspNetCore.SignalR (when added)
    -->
  </ItemGroup>
</Project>
```

#### RemoteControl.Agent
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\RemoteControl.Shared\RemoteControl.Shared.csproj" />
    <!-- Planned packages:
         - Microsoft.AspNetCore.SignalR.Client
         - System.Drawing.Common (for screenshot)
    -->
  </ItemGroup>
</Project>
```

#### RemoteControl.Shared
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <!-- No external dependencies - pure C# classes -->
</Project>
```

---

## Code Metrics Summary

### Lines of Code (Approximate)

| Project | Files | Lines | Blanks | Comments | Code |
|---------|-------|-------|--------|----------|------|
| RemoteControl.Web | 30+ | ~4000 | ~600 | ~400 | ~3000 |
| RemoteControl.Shared | 3 | 452 | 60 | 120 | 272 |
| RemoteControl.Agent | 1 | ~10 | ~5 | 0 | ~5 |
| **Total** | **34+** | **~4500** | **~665** | **~520** | **~3300** |

### Components Breakdown

| Type | Count | Total Lines |
|------|-------|-------------|
| Layout Components | 6 | ~1200 |
| Page Components | 7 | ~1500 |
| Shared Components | 8 | ~1000 |
| Models | 3 | 452 |
| Config Files | 5 | ~200 |

---

## Naming Conventions

### Files
- **Components**: PascalCase matching class name (`DeviceCard.razor`)
- **Services**: `[Feature]Service.cs` (`ScreenshotService.cs`)
- **Models**: `[Entity].cs` (`AgentInfo.cs`)
- **Hubs**: `[Feature]Hub.cs` (`RemoteControlHub.cs`)

### Classes
- **PascalCase**: `public class AgentInfo`
- **Interfaces**: `I` prefix (`IAgentManager`)
- **Records**: Same as classes (`public record AgentActivityModel`)

### Methods
- **PascalCase**: `public void RegisterAgent()`
- **Async suffix**: `public async Task RegisterAgentAsync()`

### Properties
- **PascalCase**: `public string AgentId { get; set; }`

### Variables
- **camelCase**: `var agentInfo = new AgentInfo();`
- **Private fields**: `_` prefix (`private readonly List<AgentInfo> _agents`)

---

## Folder Organization Logic

### Components/
- **Layout/**: Components dùng trong MainLayout, global UI structure
- **Pages/**: Routable pages (có `@page` directive)
- **Shared/**: Reusable components (không có `@page`)

**Rule**: Nếu component có route → Pages/, nếu không → Shared/ hoặc Layout/

### Services/ (Planned)
- Business logic classes
- Không có UI components
- Injectable via DI

### Hubs/ (Planned)
- SignalR Hub classes only
- Inherit from `Hub` base class

---

## File Size Guide

### Large Files (>1000 bytes)
- `Home.razor` (7071 bytes) - Dashboard with多 UI elements
- `CommandRequest.cs` (4258 bytes) - 22 enum values + documentation
- `CommandResult.cs` (4713 bytes) - 7 classes
- `ReconnectModal.razor.css` (3978 bytes) - Scoped styles
- `ResourceUsageCard.razor` - Chart display logic

### Medium Files (500-1000 bytes)
- Most .razor components
- Model classes với property XML docs

### Small Files (<500 bytes)
- Simple components (`NavItem.razor`)
- Config files (`tailwind.config.js`)
- Entry points (`Program.cs`)

---

## Code Organization Best Practices (Current)

### ✅ Good Practices Observed
- Separation of concerns (Components ≠ Services ≠ Models)
- Consistent naming conventions
- XML documentation on  models
- Project references (not package references) for Shared
- Tailwind utilities (not custom CSS)

### 📋 Improvements Needed
- Add unit tests
- Implement services layer
- Add error boundaries
- Logging infrastructure
- Configuration management (dev vs prod)

---

## Quick File Reference

**Want to...** → **Edit this file**:
- Change dashboard layout → `Components/Pages/Home.razor`
- Add new nav item → `Components/Layout/NavMenu.razor` + `Components/Shared/NavItem.razor`
- Create new model → `RemoteControl.Shared/Models/[YourModel].cs`
- Configure Tailwind → `tailwind.config.js`
- Add middleware → `RemoteControl.Web/Program.cs`
- Implement screenshot → `RemoteControl.Agent/Services/ScreenshotService.cs` (to create)
- Handle commands → `RemoteControl.Agent/Handlers/CommandHandler.cs` (to create)
- Create SignalR Hub → `RemoteControl.Web/Hubs/RemoteControlHub.cs` (to create)

---

**Cập Nhật**: 02/12/2024  
**Phiên Bản**: 1.0  
**Total Documentation**: ~6500 lines across 10 files

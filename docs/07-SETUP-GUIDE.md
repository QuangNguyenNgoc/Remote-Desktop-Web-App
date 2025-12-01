# Hướng Dẫn Setup Môi Trường - Remote Control Desktop

## Yêu Cầu Hệ Thống

### Phần Mềm Bắt Buộc

- [ ] **Visual Studio Community 2022** (hoặc Professional/Enterprise)
  - Workload: "ASP.NET and web development"
  - Workload: ".NET desktop development"
  - Version: 17.8+
  
- [ ] **.NET SDK 10.0**
  - Download: https://dotnet.microsoft.com/download/dotnet/10.0
  - Verify: `dotnet --version` (should show 10.0.x)

- [ ] **Node.js & npm**
  - Version: Node 18+ LTS recommended
  - Download: https://nodejs.org/
  - Verify: `node --version` và `npm --version`

- [ ] **Git**
  - Download: https://git-scm.com/
  - Verify: `git --version`

### Phần Mềm Tùy Chọn

- [ ] **VS Code** (alternative to Visual Studio)
  - Extensions: C# Dev Kit, Tailwind CSS IntelliSense
  
- [ ] **Docker Desktop** (for containerization)
  - Only if planning to deploy via Docker

- [ ] **Postman** / **Thunder Client** (API testing)

---

## VS Code Extensions (Recommended)

### Essential Extensions

```bash
# Install via VS Code Extensions panel hoặc CLI
code --install-extension ms-dotnettools.csdevkit
code --install-extension bradlc.vscode-tailwindcss
```

**List Extensions**:
- **C# Dev Kit** (`ms-dotnettools.csdevkit`) - IntelliSense, debugging
- **Tailwind CSS IntelliSense** (`bradlc.vscode-tailwindcss`) - Autocomplete cho Tailwind
- **GitLens** (`eamodio.gitlens`) - Git visualization
- **Better Comments** (`aaron-bond.better-comments`) - Color-coded comments
- **File Nesting Updater** (`antfu.file-nesting`) - Clean file tree

### Recommended Settings

```json
// .vscode/settings.json
{
  "editor.formatOnSave": true,
  "tailwindCSS.experimental.classRegex": [
    ["class:\\s*\"([^\"]*)\"", "([a-zA-Z0-9\\-:/ ]+)"]
  ]
}
```

---

## Bước 1: Clone Repository

```powershell
# Clone project về máy
git clone https://github.com/[YOUR-USERNAME]/RemoteControlProject.git

# Di chuyển vào folder
cd RemoteControlProject

# Kiểm tra branches
git branch -a

# Checkout branch develop (nếu có)
git checkout develop
```

**Cấu Trúc Sau Khi Clone**:
```
RemoteControlProject/
├── .git/
├── .gitignore
├── RemoteControl.Web/
├── RemoteControl.Agent/
├── RemoteControl.Shared/
├── docs/
├── tests/
└── RemoteControl.slnx
```

---

## Bước 2: Restore NuGet Packages

```powershell
# Restore packages cho toàn bộ solution
dotnet restore

# Hoặc restore cho từng project
dotnet restore RemoteControl.Web/RemoteControl.Web.csproj
dotnet restore RemoteControl.Agent/RemoteControl.Agent.csproj
dotnet restore RemoteControl.Shared/RemoteControl.Shared.csproj
```

**Output Kỳ Vọng**:
```
Determining projects to restore...
Restored D:\...\RemoteControl.Web\RemoteControl.Web.csproj (in 2.5 sec).
Restored D:\...\RemoteControl.Shared\RemoteControl.Shared.csproj (in 1.1 sec).
```

**Troubleshooting**:
- Nếu lỗi `NuGet package not found`: Chạy `dotnet restore --force`
- Nếu lỗi network: Check proxy settings

---

## Bước 3: Install npm Dependencies (Tailwind CSS)

```powershell
# Di chuyển vào Web project
cd RemoteControl.Web

# Install packages
npm install

# Xác nhận installed
npm list
```

**Expected Packages** (`node_modules/`):
```
RemoteControl.Web/node_modules/
├── @tailwindcss/cli@4.1.17
└── tailwindcss@4.1.17
```

**Troubleshooting**:
- Lỗi `npm ERR!`: Delete `node_modules/` và `package-lock.json`, chạy lại `npm install`
- Lỗi permissions: Run PowerShell as Administrator

---

## Bước 4: Configure appsettings (Optional)

```powershell
# Web project settings
cd RemoteControl.Web

# Copy template nếu có
# cp appsettings.template.json appsettings.Development.json

# Edit settings
code appsettings.json  # Hoặc notepad appsettings.json
```

**appsettings.json** - Mặc định:
```json
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

**Future Settings** (khi thêm SignalR, Database):
```json
{
  "SignalR": {
    "HubUrl": "/remotehub",
    "MaxMessageSize": 32768
  },
  "ConnectionStrings": {
    "DefaultConnection": "..."
  }
}
```

---

## Bước 5: Build Project

### Build Solution

```powershell
# Quay về project root
cd ..  # (Nếu đang ở RemoteControl.Web)

# Build toàn bộ solution
dotnet build RemoteControl.slnx

# Hoặc build từng project riêng
dotnet  build RemoteControl.Web/RemoteControl.Web.csproj
dotnet build RemoteControl.Agent/RemoteControl.Agent.csproj
dotnet build RemoteControl.Shared/RemoteControl.Shared.csproj
```

**Output Success**:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:05.23
```

**Common Errors**:
- `CS0246: The type or namespace name ... could not be found`
  → Run `dotnet restore` again
- `error MSB4236: The SDK 'Microsoft.NET.Sdk.Web' ... could not be found`
  → Install .NET 10.0 SDK

---

## Bước 6: Build Tailwind CSS

```powershell
cd RemoteControl.Web

# Build CSS một lần
npm run watch  # This watches for changes
```

**Output**:
```
Rebuilding...
Done in 142ms.
```

**Files Generated**:
- `RemoteControl.Web/wwwroot/css/app.css` (compiled Tailwind)

**Tip**: Mở Terminal riêng cho `npm run watch` để auto-rebuild CSS khi sửa file `.razor`

---

## Bước 7: Run Web Application

### Option 1: Visual Studio

1. Open `RemoteControl.slnx` in Visual Studio 2022
2. Right-click `RemoteControl.Web` > Set as Startup Project
3. Press `F5` (hoặc click Start Debugging)

**Browser Tự Động Mở**: `https://localhost:7102` hoặc `http://localhost:5048`

### Option 2: Command Line (dotnet watch)

```powershell
cd RemoteControl.Web

# Run with hot reload
dotnet watch

# manual run (không hot reload)
# dotnet run
```

**Output**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5048
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7102
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shutdown.
```

**Truy Cập**: Mở browser đến `http://localhost:5048` hoặc `https://localhost:7102`

### Hot Reload với dotnet watch

**Khi sửa file `.razor` hoặc `.cs`**:
```
watch : File changed: Components/Pages/Home.razor
watch : Building...
watch : Succeeded. No browser refresh required.
```

**Tự động refresh browser** - không cần F5!

---

## Bước 8: Run Agent (Planned)

```powershell
# Terminal riêng cho Agent
cd RemoteControl.Agent

# Run agent
dotnet run

# Hoặc với arguments
dotnet run -- --server http://localhost:5048/remotehub
```

**Expected Output** (khi implement):
```
Agent starting...
Connecting to Hub: http://localhost:5048/remotehub
Agent registered with ID: a1b2c3-d4e5-...
Ready to receive commands.
```

---

## Development Workflow

### Daily Workflow

```powershell
# 1. Pull latest changes
git pull origin main

# 2. Open 2 terminals side-by-side

# Terminal 1: Tailwind watch
cd RemoteControl.Web
npm run watch

# Terminal 2: Web app hot reload
cd RemoteControl.Web
dotnet watch

# 3. Làm việc trong VS Code hoặc Visual Studio
# 4. Browser tự refresh khi save file
# 5. Commit changes
git add .
git commit -m "feat: add new feature"
git push
```

### Recommended Terminal Layout

```
┌─────────────────────────────┬─────────────────────────────┐
│  Terminal 1                 │  Terminal 2                 │
│  npm run watch              │  dotnet watch               │
│  (Tailwind auto-compile)    │  (Web app hot reload)       │
│                             │                             │
│  Rebuilding...              │  Now listening on:          │
│  Done in 42ms.              │  http://localhost:5048      │
└─────────────────────────────┴─────────────────────────────┘
```

---

## Troubleshooting

### Common Issues

#### 1. Port Already in Use

**Error**:
```
System.IO.IOException: Failed to bind to address http://localhost:5048
```

**Solution**:
```powershell
# Tìm process đang dùng port 5048
netstat -ano | findstr :5048

# Kill process theo PID
taskkill /PID [PID_NUMBER] /F

# Hoặc đổi port trong launchSettings.json
```

#### 2. Tailwind CSS Không Generate

**Symptoms**: Styles không apply, file `app.css` empty hoặc outdated

**Solution**:
```powershell
# Stop npm run watch (Ctrl+C)
# Delete cache
Remove-Item node_modules -Recurse -Force
Remove-Item package-lock.json

# Reinstall
npm install

# Rebuild
npm run watch
```

#### 3. NuGet Restore Fails

**Error**: `error NU1101: Unable to find package`

**Solution**:
```powershell
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore with verbose logging
dotnet restore --verbosity detailed

# Force restore
dotnet restore --force
```

#### 4. Hot Reload Không Hoạt Động

**Solution**:
```powershell
# Ensure dotnet watch (not dotnet run)
dotnet watch

# Check launchSettings.json
# Ensure ASPNETCORE_ENVIRONMENT=Development
```

#### 5. SignalR Connection Failed (Future)

**Error**: `Failed to start the connection: Error: ...`

**Solution**:
- Check Hub URL in appsettings.json
- Ensure `app.MapHub<RemoteControlHub>("/remotehub")` in Program.cs
- Check browser console for CORS errors
- Verify Antiforgery token

---

## Git Configuration

### Branching Strategy

```
main (or master)          # Production-ready code
  │
  ├─ develop              # Integration branch
  │   │
  │   ├─ feature/screenshot      # New features
  │   ├─ feature/keylogger
  │   ├─ bugfix/login-error      # Bug fixes
  │   └─ hotfix/critical-bug     # Urgent fixes
```

### Commit Message Convention

```bash
# Format
<type>(<scope>): <subject>

# Types
feat:     New feature
fix:      Bug fix
docs:     Documentation
style:    Formatting (no code change)
refactor: Code restructuring
test:     Adding tests
chore:    Build/maintenance tasks

# Examples
git commit -m "feat(agent): add screenshot service"
git commit -m "fix(hub): resolve connection timeout"
git commit -m "docs(setup): add troubleshooting section"
```

### Pre-commit Checks

```powershell
# Before committing
dotnet build         # Ensure builds
dotnet test          # Run tests (when implemented)
```

---

## Next Steps

### After Setup

1. **Verify Dashboard**: Mở `http://localhost:5048` → Should see Home page với stats
2. **Check Components**: Navigate `/device-manager`, `/device-control`
3. **Read Documentation**: Xem `docs/01-ARCHITECTURE.md` để hiểu hệ thống
4. **Start Implementing**: Follow `docs/06-FEATURES-IMPLEMENTATION.md`

### Implement SignalR (Phase 8)

```powershell
# 1. Create Hub
code RemoteControl.Web/Hubs/RemoteControlHub.cs

# 2. Register in Program.cs
# builder.Services.AddSignalR();
# app.MapHub<RemoteControlHub>("/remotehub");

# 3. Test connection
```

### Implement Agent Services (Phase  9)

```powershell
# 1. Create ScreenshotService
code RemoteControl.Agent/Services/ScreenshotService.cs

# 2. Implement command handlers
code RemoteControl.Agent/Handlers/CommandHandler.cs
```

---

## Quick Reference Commands

```powershell
# Restore packages
dotnet restore

# Build
dotnet build

# Run Web (hot reload)
cd RemoteControl.Web
dotnet watch

# Run Agent
cd RemoteControl.Agent
dotnet run

# Tailwind watch
cd RemoteControl.Web
npm run watch

# Git
git status
git add .
git commit -m "message"
git push
```

---

## Learning Resources

### Official Docs
- [Blazor Tutorial](https://learn.microsoft.com/en-us/aspnet/core/blazor/tutorials/build-a-blazor-app)
- [SignalR with Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/tutorials/signalr-blazor)
- [Tailwind CSS](https://tailwindcss.com/docs)

### Video Tutorials
- [Blazor Course - FreeCodeCamp](https://www.freecodecamp.org/news/blazor-course/)
- [SignalR Real-time Apps](https://www.youtube.com/results?search_query=signalr+tutorial)

### Community
- [Blazor Discord](https://discord.gg/blazor)
- [Stack Overflow - Blazor Tag](https://stackoverflow.com/questions/tagged/blazor)

---

**Cập Nhật**: 02/12/2024  
**Phiên Bản**: 1.0

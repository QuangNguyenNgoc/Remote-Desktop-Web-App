# 🖥️ Remote Control Desktop Project

Hệ thống điều khiển máy tính từ xa sử dụng Blazor Web App và SignalR.

## 🎯 Tổng quan

Project bao gồm 3 thành phần chính:
- **RemoteControl.Web**: Blazor Web App (Frontend + Backend) với SignalR Hub
- **RemoteControl.Agent**: Desktop agent chạy trên máy client
- **RemoteControl.Shared**: Shared library chứa models và constants chung

## 📁 Cấu trúc Project

```
RemoteControlProject/
├── RemoteControl.Web/       # 🌐 Blazor Web App
│   ├── Components/          # Blazor components
│   ├── Hubs/               # SignalR hubs
│   ├── Services/           # Backend services
│   └── wwwroot/            # Static files (CSS, JS)
│
├── RemoteControl.Agent/     # 🤖 Desktop Agent
│   ├── Services/           # System services (screenshot, keylog, etc.)
│   ├── Models/             # Data models
│   └── Handlers/           # Command handlers
│
├── RemoteControl.Shared/    # 🔗 Shared Library
│   ├── Models/             # Shared data models
│   └── Constants/          # SignalR event names, etc.
│
├── tests/                   # 🧪 Unit tests
├── docker/                  # 🐳 Docker configs (TODO)
├── docs/                    # 📚 Documentation
└── RemoteControl.sln        # Visual Studio Solution
```

## 🚀 Bắt đầu

### Prerequisites

- .NET 10.0 SDK
- Node.js (cho Tailwind CSS)

### Chạy Web App

```powershell
cd RemoteControl.Web
dotnet watch
```

Web app sẽ chạy tại `https://localhost:5001`

### Chạy Agent (TODO)

```powershell
cd RemoteControl.Agent
dotnet run
```

## 🛠️ Công nghệ

- **Frontend**: Blazor Server với Tailwind CSS
- **Backend**: ASP.NET Core
- **Real-time**: SignalR
- **Agent**: C# Console App

## 📖 Documentation

Xem thư mục [docs/](./docs/) để biết thêm chi tiết về:
- Component guides
- Architecture
- SignalR flow
- Setup guide

## 🎨 Features (Planned)

### Web Dashboard
- ✅ Danh sách devices/agents
- ✅ Real-time status monitoring
- 🔜 Remote screenshot viewing
- 🔜 Command panel
- 🔜 File management

### Agent Capabilities
- 🔜 Screenshot capture
- 🔜 Keyboard monitoring
- 🔜 Process management
- 🔜 File system operations
- 🔜 System info reporting

## 📝 TODO

- [ ] Triển khai RemoteControl.Agent project
- [ ] Triển khai RemoteControl.Shared library
- [ ] Di chuyển models sang Shared
- [ ] Implement SignalR communication
- [ ] Add authentication & authorization
- [ ] Docker containerization

## 📄 License

[Thêm license nếu cần]

---

**Status**: 🟢 Active Development

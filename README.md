# 🖥️ Remote Control Desktop

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-Server-512BD4?style=flat-square&logo=blazor)](https://blazor.net/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](./LICENSE)
[![Status](https://img.shields.io/badge/Status-Active-success?style=flat-square)](https://github.com/QuangNguyenNgoc/Remote-Desktop-Web-App)

**Hệ thống điều khiển máy tính từ xa** sử dụng Blazor Web App, SignalR và C# Agent.

## ✨ Features

### Web Dashboard
- ✅ Real-time device monitoring với SignalR
- ✅ Live screenshot streaming
- ✅ Task Manager (xem/diệt processes)
- ✅ System monitoring (CPU, RAM, Disk)
- ✅ Registry browser
- ✅ Passkey authentication
- ✅ Rate limiting & Security headers

### Agent Capabilities
- ✅ Screenshot capture
- ✅ Process management
- ✅ System info reporting
- ✅ Webcam streaming
- ✅ Stealth mode (hidden execution)
- ✅ Windows Service support
- ✅ Auto-reconnect

---

## 🚀 Quick Start

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (for Tailwind CSS)
- Git

### 1. Clone & Install

```bash
git clone https://github.com/QuangNguyenNgoc/Remote-Desktop-Web-App.git
cd Remote-Desktop-Web-App

# Install dependencies
dotnet restore
cd RemoteControl.Web && npm install && cd ..
```

### 2. Run Web Server

```bash
# Terminal 1: Tailwind CSS
cd RemoteControl.Web
npm run watch

# Terminal 2: Web server
cd RemoteControl.Web
dotnet watch
```

Open: `https://localhost:5049`

### 3. Run Agent

```bash
cd RemoteControl.Agent
dotnet run
```

> Xem **[Build & Publish Guide](./docs/BUILD-AND-PUBLISH-GUIDE.md)** để biết thêm về: Hidden mode, Windows Service, Ngrok deployment.

---

## 📁 Project Structure

```
Remote-Desktop-Web-App/
├── RemoteControl.Web/       # 🌐 Blazor Web Server
│   ├── Components/          # Blazor components
│   ├── Hubs/                # SignalR hub
│   ├── Middleware/          # Security headers, rate limiting
│   └── Services/            # Dashboard state, toast, etc.
│
├── RemoteControl.Agent/     # 🤖 Windows Agent
│   ├── Services/            # Screenshot, process, webcam, etc.
│   ├── Handlers/            # Command handler
│   ├── Helpers/             # Stealth helper
│   └── Workers/             # Background service worker
│
├── RemoteControl.Shared/    # 🔗 Shared Library
│   ├── Models/              # Request/Response models
│   └── Constants/           # SignalR event names
│
└── docs/                    # 📚 Documentation
```

---

## 🛠️ Tech Stack

| Component | Technology |
|-----------|------------|
| Frontend | Blazor Server + Tailwind CSS |
| Backend | ASP.NET Core 10.0 |
| Real-time | SignalR |
| Agent | C# + Windows Forms |
| Streaming | WebSocket |

---

## 📖 Documentation

- **[Build & Publish Guide](./docs/BUILD-AND-PUBLISH-GUIDE.md)** - Local dev, deployment, ngrok
- **[Architecture](./docs/01-ARCHITECTURE.md)** - System design
- **[SignalR Protocol](./docs/05-SIGNALR-PROTOCOL.md)** - Hub events & messages
- **[Security Guide](./docs/10-SECURITY-GUIDE.md)** - Authentication, headers

---

## 🐳 Docker (Web Server Only)

> Agent là Windows-specific, chỉ Web server hỗ trợ Docker.

```bash
# Option 1: Docker Compose (recommended)
docker-compose up -d

# Option 2: Manual build & run
docker build -t remotecontrol-web -f RemoteControl.Web/Dockerfile .
docker run -d -p 5048:5048 remotecontrol-web
```

Access: `http://localhost:5048`

Xem [Build & Publish Guide](./docs/BUILD-AND-PUBLISH-GUIDE.md) để biết thêm.

---

## 🌐 Remote Access via Ngrok

```bash
# Expose Web server to internet (port 5048!)
ngrok http 5048

# Agent config (appsettings.json)
# HubUrl: "https://abc123.ngrok-free.dev/remotehub"
# Note: Ngrok URL không có port!
```

---

## 📄 License

This project is licensed under the [MIT License](./LICENSE).

---

## 🤝 Contributing

1. Fork the repo
2. Create feature branch (`git checkout -b feature/amazing`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push (`git push origin feature/amazing`)
5. Open Pull Request

---

**Made with ❤️ by [Nguyễn Ngọc Quang](https://github.com/QuangNguyenNgoc), [Đặng Quang Tiến](https://github.com/F0n9), [Nguyễn Hưng](https://github.com/orzADDICT169)**

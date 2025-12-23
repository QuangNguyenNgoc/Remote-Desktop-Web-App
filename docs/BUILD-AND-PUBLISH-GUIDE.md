# Build and Publish Guide

Hướng dẫn build và deploy ứng dụng Remote Control Desktop.

---

## 📋 Mục lục

1. [Local Development](#1-local-development)
2. [Agent Build Modes](#2-agent-build-modes)
3. [Publish via Ngrok](#3-publish-via-ngrok)
4. [Production Build](#4-production-build)

---

## 1. Local Development

### Prerequisites

```powershell
# Check .NET SDK
dotnet --list-sdks

# Check Node.js (cho Tailwind CSS)
node --version
npm --version
```

### Chạy Web Server

```powershell
# Terminal 1: Tailwind CSS watch
cd RemoteControl.Web
npm run watch

# Terminal 2: Web server
cd RemoteControl.Web
dotnet watch
```

Web sẽ chạy tại `https://localhost:5048`

### Chạy Agent (Debug Mode)

```powershell
cd RemoteControl.Agent
dotnet run
```

Dialog sẽ hiện ra → nhập IP server → Connect.

---

## 2. Agent Build Modes

### 2.1 Debug Mode (Development)

```powershell
cd RemoteControl.Agent
dotnet run
```

- Hiện WinForms UI với dialog nhập IP
- Có console output để debug

### 2.2 Hidden Mode (Stealth)

Build và chạy ẩn, không có UI:

```powershell
# Build
cd RemoteControl.Agent
dotnet build -c Release

# Chạy hidden (command line)
.\bin\Release\net10.0-windows\RemoteControl.Agent.exe --mode=hidden --server=192.168.1.100:5048
```

**Hoặc cấu hình trong `appsettings.json`:**

```json
{
  "SignalR": {
    "HubUrl": "http://192.168.1.100:5048/remotehub"
  },
  "Agent": {
    "DefaultMode": "Hidden"
  }
}
```

**Mode priority:** Command line args > appsettings.json > Default (Debug)

### 2.3 Self-Contained Publish (Portable)

Tạo file exe độc lập, không cần cài .NET:

```powershell
cd RemoteControl.Agent

# Standard publish
dotnet publish -c Release -r win-x64 --self-contained

# Single file (recommended for distribution)
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:DebugType=None
```

Output: `bin\Release\net10.0-windows\win-x64\publish\`

**Lưu ý:** `appsettings.json` được copy vào publish folder, chỉ cần sửa file này và zip gửi user.

### 2.4 Windows Service

```powershell
# Publish
dotnet publish -c Release -r win-x64 --self-contained

# Install (Admin PowerShell)
sc.exe create "RemoteControlAgent" binPath="C:\path\to\Agent.exe --mode=service --server=192.168.1.100:5048" start=auto

# Start/Stop/Delete
sc.exe start RemoteControlAgent
sc.exe stop RemoteControlAgent
sc.exe delete RemoteControlAgent
```

---

## 3. Publish via Ngrok

Ngrok cho phép expose localhost ra internet để test từ xa.

### 3.1 Cài đặt Ngrok

1. Tải từ [ngrok.com](https://ngrok.com/download) hoặc `winget install ngrok.ngrok`
2. Đăng ký tài khoản (miễn phí)
3. Chạy `ngrok authtoken <your-token>`

### 3.2 Expose Web Server

```powershell
# Terminal 1: Chạy Web (nếu chưa chạy)
cd RemoteControl.Web
dotnet watch

# Terminal 2: Expose via Ngrok (PORT 5048, không phải 5049!)
ngrok http 5048
```

Ngrok sẽ cung cấp URL như: `https://abc123.ngrok-free.dev`

### 3.3 Connect Agent từ máy khác

**⚠️ QUAN TRỌNG:** URL ngrok KHÔNG có port!

```json
// appsettings.json
{
  "SignalR": {
    "HubUrl": "https://abc123.ngrok-free.dev/remotehub"
  },
  "Agent": {
    "DefaultMode": "Hidden"
  }
}
```

**Sai:** `https://abc123.ngrok-free.dev:5048/remotehub` ❌
**Đúng:** `https://abc123.ngrok-free.dev/remotehub` ✅

### 3.4 Performance Notes

| Scenario | FPS Expected | Latency |
|----------|--------------|----------|
| **LAN direct** (IP:5048) | 15-20 FPS | <10ms |
| **Ngrok** (qua Internet) | 1-5 FPS | 80-150ms |

**Lý do Ngrok chậm:**
- Data đi qua ngrok servers dù cùng mạng
- Free plan có bandwidth limit
- Base64 frames ~200-500KB mỗi frame

**Recommend:**
- **LAN:** Dùng IP trực tiếp (mở firewall port 5048)
- **WAN:** Dùng Ngrok (chấp nhận FPS thấp) hoặc VPN

### ⚠️ Lưu ý Ngrok

- URL miễn phí thay đổi mỗi lần restart
- Session miễn phí giới hạn vài giờ
- Nên dùng cho testing, không phải production

---

## 4. Production Build

### 4.1 Web Server (Self-Contained)

```powershell
cd RemoteControl.Web
dotnet publish -c Release -r linux-x64 --self-contained
# hoặc
dotnet publish -c Release -r win-x64 --self-contained
```

### 4.2 Web Server (Docker)

> 📝 Xem [Issue #26 - Docker Containerization](./issues/26-Docker-Containerization.md)

```powershell
# Build image (từ thư mục gốc)
docker build -t remotecontrol-web -f RemoteControl.Web/Dockerfile .

# Run container
docker run -d -p 5048:5048 remotecontrol-web
```

### 4.3 Agent Distribution

Để triển khai Agent cho end-users:

1. **Build self-contained (single file):**
   ```powershell
   cd RemoteControl.Agent
   dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:DebugType=None
   ```

2. **Sửa `appsettings.json`** trong folder publish:
   ```json
   {
     "SignalR": {
       "HubUrl": "http://your-server:5048/remotehub"
     },
     "Agent": {
       "DefaultMode": "Hidden"
     }
   }
   ```

   **Với Ngrok:**
   ```json
   {
     "SignalR": {
       "HubUrl": "https://your-ngrok-url.ngrok-free.dev/remotehub"
     },
     "Agent": {
       "DefaultMode": "Hidden"
     }
   }
   ```

3. **Zip folder publish:**
   ```powershell
   Compress-Archive -Path "bin\Release\net10.0-windows\win-x64\publish\*" -DestinationPath "RemoteControlAgent.zip" -Force
   ```

4. **User chỉ cần giải nén và double-click** `RemoteControl.Agent.exe`

---

## 📁 Quick Reference

| Task | Command |
|------|---------|
| Run Web (dev) | `cd RemoteControl.Web && dotnet watch` |
| Run Agent (debug) | `cd RemoteControl.Agent && dotnet run` |
| Run Agent (hidden) | `Agent.exe --mode=hidden --server=IP:PORT` |
| Build Agent portable | `dotnet publish -c Release -r win-x64 --self-contained` |
| Build Agent single file | `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true` |
| Expose via Ngrok | `ngrok http 5048` |

## 📋 appsettings.json Template

```json
{
  "SignalR": {
    "HubUrl": "http://192.168.1.100:5048/remotehub",
    "ReconnectInterval": 5000
  },
  "Agent": {
    "DefaultMode": "Debug"
  }
}
```

**DefaultMode values:** `Debug`, `Hidden`, `Service`

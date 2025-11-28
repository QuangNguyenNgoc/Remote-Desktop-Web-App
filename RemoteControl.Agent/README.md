# RemoteControl.Agent

🤖 Desktop Agent cho Remote Control System.

## Mô tả

Agent chạy trên máy tính client, kết nối với RemoteControl.Web qua SignalR để nhận lệnh điều khiển từ xa.

## Tính năng (Sẽ triển khai)

- 📸 Screenshot màn hình
- ⌨️ Keyboard monitoring
- 🔄 Process management
- 📁 File system operations
- 💻 System information (CPU, RAM, etc.)

## Cấu trúc

```
RemoteControl.Agent/
├── Services/         # Core services
├── Models/          # Data models (shared với Web)
├── Handlers/        # Command handlers
├── Program.cs       # Entry point & SignalR client
└── appsettings.json # Configuration
```

## Cách chạy

```powershell
cd RemoteControl.Agent
dotnet run
```

## TODO

- [ ] Tạo project file (.csproj)
- [ ] Implement SignalR client connection
- [ ] Implement command handlers
- [ ] Add system services

# RemoteControl.Shared

🔗 Shared library cho Remote Control System.

## Mô tả

Library chứa các models và constants dùng chung giữa Web và Agent projects.

## Cấu trúc

```
RemoteControl.Shared/
├── Models/          # Shared data models
│   ├── AgentInfo.cs
│   ├── CommandRequest.cs
│   ├── DeviceStatus.cs
│   └── ScreenshotData.cs
└── Constants/
    └── SignalREvents.cs  # Event names
```

## TODO

- [ ] Tạo project file (.csproj)
- [ ] Di chuyển models từ Web sang đây
- [ ] Thêm constants cho SignalR events

# Issue #17: [Agent] Create CommandHandler

### 🎯 Mục Tiêu

Tạo handler route commands từ Hub đến các services

### ✅ Checklist

- [ ] Tạo file `RemoteControl.Agent/Handlers/CommandHandler.cs`
- [ ] Inject các services: `ScreenshotService`, `ProcessService`, `SystemInfoService`
- [ ] Implement `HandleCommand(CommandRequest request)` với switch case:
  - `CaptureScreen` → call `ScreenshotService.CaptureScreen()`
  - `ListProcesses` → call `ProcessService.ListProcesses()`
  - `KillProcess` → call `ProcessService.KillProcess(pid)`
  - `StartProcess` → call `ProcessService.StartProcess(name)`
  - `GetSystemInfo` → call `SystemInfoService.GetSystemInfo()`
- [ ] Return `CommandResult` với data phù hợp
- [ ] Add error handling cho unknown commands
- [ ] Add logging

### 🔗 Dependencies

- ✅ #14: ScreenshotService (Done)
- ✅ #15: ProcessService (Done)
- ⏳ #16: SystemInfoService (Đang làm)

### 📝 Notes

- Sử dụng `CommandType` enum từ Shared
- Return types phải khớp với Shared models (`ScreenshotResult`, `ProcessListResult`, etc.)

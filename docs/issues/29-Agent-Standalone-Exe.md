# Issue #29: [Agent] Publish as Standalone Executable

### 🎯 Mục Tiêu

Đóng gói Agent thành .exe có thể chạy độc lập không cần cài .NET

### ✅ Checklist

**Build Configuration:**
- [ ] Thêm publish profile trong project
- [ ] Build self-contained executable cho Windows x64
- [ ] Single file output (tùy chọn)
- [ ] Trim unused libraries để giảm size

**Configuration:**
- [ ] Đọc Hub URL từ `appsettings.json` hoặc command line
- [ ] Đọc Passkey/Token từ config
- [ ] Support command line arguments

**Installer (Optional):**
- [ ] Tạo installer với Inno Setup hoặc WiX
- [ ] Auto-start with Windows option
- [ ] Desktop shortcut

**Testing:**
- [ ] Test trên máy không có .NET SDK
- [ ] Verify tất cả features hoạt động
- [ ] Document file size và requirements

### 🔗 Dependencies

- ✅ #17: CommandHandler hoàn thành
- ✅ #18: Agent SignalR connection

### 📝 Build Command

```powershell
cd RemoteControl.Agent

# Self-contained, single file
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish

# Output: publish/RemoteControl.Agent.exe (~50-80 MB)
```

### 📦 Runtime Requirements

- Windows 10 x64 trở lên
- .NET Desktop Runtime (nếu không self-contained)
- Quyền Administrator (cho một số features như keylogger)

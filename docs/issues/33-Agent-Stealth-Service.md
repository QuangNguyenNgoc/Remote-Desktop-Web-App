# Issue #33: [Agent] Stealth Mode & Windows Service

### 🎯 Mục Tiêu

Agent chạy ẩn và như Windows Service

### ✅ Checklist

**Stealth Mode:**
- [ ] Ẩn console window khi chạy
- [ ] Không hiển thị trong taskbar
- [ ] Run as background process
- [ ] Option để toggle hiện/ẩn

**Windows Service:**
- [ ] Chuyển đổi thành Windows Service
- [ ] Install/Uninstall commands
- [ ] Auto-start với Windows
- [ ] Run as LocalSystem account

**Persistence:**
- [ ] Registry autorun (optional)
- [ ] Scheduled task (optional)
- [ ] Service recovery settings

### 🔗 Dependencies

- ⏳ #29: Agent standalone exe

### 📝 Notes

- ⚠️ Feature này có thể bị antivirus flag
- Chỉ dùng cho mục đích hợp pháp (IT management)
- Cần quyền Administrator để cài service

### 📦 Service Install Command

```powershell
# Install as service
sc.exe create "RemoteControlAgent" binPath="C:\Path\To\Agent.exe" start=auto

# Uninstall
sc.exe delete "RemoteControlAgent"
```

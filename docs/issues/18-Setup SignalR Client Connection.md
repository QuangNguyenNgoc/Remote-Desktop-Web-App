# Issue #18: [Agent] Setup SignalR Client Connection

### 🎯 Mục Tiêu

Kết nối Agent tới Hub qua SignalR và xử lý commands

### ✅ Checklist

**Connection Setup:**
- [ ] Add `Microsoft.AspNetCore.SignalR.Client` package
- [ ] Tạo `HubConnection` với URL `http://localhost:5048/remotehub`
- [ ] Add `WithAutomaticReconnect()` cho auto-reconnect
- [ ] Handle connection events: `Reconnecting`, `Reconnected`, `Closed`

**Agent Registration:**
- [ ] Tạo `AgentInfo` object với thông tin máy (MachineName, IP, OS)
- [ ] Gọi `RegisterAgent(AgentInfo)` sau khi connect thành công
- [ ] Implement heartbeat loop (mỗi 10s gọi `Heartbeat`)

**Command Handling:**
- [ ] Subscribe event `ExecuteCommand` từ Hub
- [ ] Khi nhận command → gọi `CommandHandler.HandleCommand()`
- [ ] Gửi kết quả về Hub qua `SendResult(CommandResult)`

**Testing:**
- [ ] Chạy Web app
- [ ] Chạy Agent
- [ ] Verify logs: "Agent registered" trong Hub console
- [ ] Verify: Agent xuất hiện trong DeviceManager

### 🔗 Dependencies

- ✅ #10, #11: Hub endpoint `/remotehub` đã có
- ⏳ #17: CommandHandler (cần hoàn thành trước)

### 📝 Notes

- Agent chạy như console app, không cần UI
- Connection string có thể đọc từ config file sau này

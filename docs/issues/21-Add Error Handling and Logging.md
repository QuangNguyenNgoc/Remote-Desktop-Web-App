# Issue #21: [QA] Error Handling & Logging

### 🎯 Mục Tiêu

Thêm error handling và logging cho Hub và Agent

### ✅ Checklist

**Hub (RemoteControlHub.cs):**
- [ ] Wrap tất cả methods trong try-catch
- [ ] Log errors với `ILogger<RemoteControlHub>`
- [ ] Return error responses thay vì throw exceptions
- [ ] Log structured data (AgentId, CommandId, etc.)

**Agent:**
- [ ] Add logging framework (Console hoặc Serilog)
- [ ] Log command received/completed
- [ ] Handle SignalR connection errors gracefully
- [ ] Implement retry logic cho failed commands

**CommandResult Improvements:**
- [ ] Thêm error codes (enum `ErrorCode`)
- [ ] Thêm detailed error messages
- [ ] Update docs với error codes

**Testing:**
- [ ] Test: Agent offline → verify error message
- [ ] Test: Invalid command type → verify handling
- [ ] Test: Permission denied (kill system process) → verify error

### 🔗 Dependencies

- ⏳ #19, #19a, #19b: Cần hoàn thành E2E tests trước
- Có thể làm song song với #22

### 📝 Notes

- Consider: Add centralized exception handler
- Consider: Log to file cho production

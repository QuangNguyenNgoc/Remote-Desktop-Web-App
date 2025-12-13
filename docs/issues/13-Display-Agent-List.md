# Issue #13: [FE+BE] Hiển Thị Danh Sách Agents Real-time

### 🎯 Mục Tiêu

Cập nhật DeviceManager.razor để hiển thị agents từ Hub + Implement Hub method `GetAllAgents`

### ✅ Checklist

**Backend (Hub):**
- [ ] Implement `GetAllAgents()` method trong `RemoteControlHub.cs`
- [ ] Return `List<AgentInfo>` từ `AgentConnections` dictionary
- [ ] Add logging cho method

**Frontend (Blazor):**
- [ ] Thêm `List<AgentInfo> Agents` field trong DeviceManager.razor
- [ ] Call `hubConnection.InvokeAsync<List<AgentInfo>>("GetAllAgents")` trong OnInitialized
- [ ] Render `DeviceCard` cho mỗi agent
- [ ] Subscribe event `AgentConnected` → thêm agent vào list
- [ ] Subscribe event `AgentDisconnected` → xóa agent khỏi list
- [ ] Thêm loading indicator khi đang fetch

### 🔗 Dependencies

- ✅ #10, #11: RemoteControlHub đã có sẵn
- ✅ Shared Models: `AgentInfo` đã có

### 📝 Notes

- `GetAllAgents` cần convert `ConcurrentDictionary` sang `List<AgentInfo>`
- Cần lưu thêm `AgentInfo` vào dictionary (không chỉ connectionId)

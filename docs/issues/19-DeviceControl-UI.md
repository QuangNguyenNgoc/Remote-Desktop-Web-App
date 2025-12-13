# Issue #19: [FE] DeviceControl UI - Buttons & Actions

### 🎯 Mục Tiêu

Thêm UI controls vào DeviceControl.razor để gửi commands đến Agent

### ✅ Checklist

**SignalR Connection:**
- [ ] Add `HubConnection` field
- [ ] Connect trong `OnInitializedAsync()`
- [ ] Subscribe events: `CommandCompleted`, `SystemInfoUpdated`

**Screenshot Actions:**
- [ ] Thêm button "📷 Capture Screenshot"
- [ ] Gọi `hubConnection.InvokeAsync("SendCommand", screenshotRequest)`
- [ ] Hiển thị screenshot trong `RemoteScreen` component khi nhận result

**Process Management UI:**
- [ ] Thêm button "📋 List Processes"
- [ ] Hiển thị process list trong table/grid
- [ ] Mỗi row có button "Kill" để kill process
- [ ] Thêm input + button "Start Process"

**System Info:**
- [ ] Hiển thị CPU/RAM usage từ agent
- [ ] Auto-refresh khi nhận `SystemInfoUpdated` event

**Loading States:**
- [ ] Disable buttons khi đang xử lý
- [ ] Show spinner khi chờ response
- [ ] Show error message nếu command fail

### 🔗 Dependencies

- ⏳ #13: DeviceManager hiển thị agent list (để navigate đến)
- ⏳ #18: Agent connected và xử lý commands

### 📝 Notes

- Device ID lấy từ route parameter `{Id}`
- Cần validate agent còn online trước khi gửi command

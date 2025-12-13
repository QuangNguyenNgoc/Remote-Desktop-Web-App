# Issue #19a: [Test] E2E Test - Screenshot Feature

### 🎯 Mục Tiêu

Test toàn bộ flow screenshot: Dashboard → Hub → Agent → Hub → Dashboard

### ✅ Checklist

**Setup:**
- [ ] Chạy Web app (`dotnet watch` trong RemoteControl.Web)
- [ ] Chạy Agent console app

**Test Steps:**
- [ ] Mở DeviceManager.razor → verify agent hiển thị
- [ ] Click vào agent → navigate đến DeviceControl
- [ ] Click "Capture Screenshot" button
- [ ] Verify: Agent console log nhận command
- [ ] Verify: Screenshot được gửi về Hub (check Hub logs)
- [ ] Verify: Screenshot hiển thị trong RemoteScreen component
- [ ] Test: Capture liên tiếp 3 lần → verify không crash

**Error Cases:**
- [ ] Test: Click capture khi agent offline → show error message
- [ ] Test: Network disconnect giữa chừng → verify reconnect

**Documentation:**
- [ ] Ghi lại video demo (optional)
- [ ] Screenshot các bước thành công

### 🔗 Dependencies

- ✅ #19: DeviceControl UI có button Capture
- ✅ #18: Agent connected và xử lý commands
- ✅ #14: ScreenshotService

### 📝 Expected Results

- Screenshot hiển thị trong < 3 giây
- Format: JPEG, quality có thể config
- Không memory leak khi capture nhiều lần

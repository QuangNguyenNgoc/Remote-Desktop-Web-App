# [FE] Refactor DeviceControl UI - Integrate Existing Components

### 🎯 Mục Tiêu

Refactor giao diện DeviceControl.razor để tích hợp đúng với các components có sẵn thay vì tạo mới, đồng thời bổ sung các tính năng còn thiếu.

---

## ✅ Phase 1: Cleanup & Restructure

**Xóa/Refactor UI thừa:**
- [ ] Review `DeviceControl.razor` - xác định components nào cần giữ/xóa
- [ ] Tích hợp vào layout system có sẵn (MainLayout, etc.)
- [ ] Sử dụng existing CSS classes thay vì inline styles
- [ ] Đảm bảo dark mode hoạt động đúng

**Restructure Components:**
- [ ] Tách các section thành child components (nếu cần)
- [ ] Đảm bảo responsive design

---

## ✅ Phase 2: Bổ Sung Features

**Webcam:**
- [ ] Thêm section Webcam trong DeviceControl
- [ ] Button Start/Stop stream
- [ ] Hiển thị video stream từ Agent
- [ ] Xử lý error khi không có camera

**Power Controls:**
- [ ] Thêm section Power Management
- [ ] Buttons: Sleep, Lock, Restart, Shutdown
- [ ] Confirm dialog trước khi thực hiện
- [ ] Hiển thị kết quả thành công/thất bại

**Keylogger:**
- [ ] Thêm section Keylogger
- [ ] Button Start/Stop logging
- [ ] Hiển thị log entries (với scroll)
- [ ] Clear log button

---

## ✅ Phase 3: Polish & UX

**Giao diện:**
- [ ] Consistent styling với phần còn lại của app
- [ ] Animations và transitions mượt mà
- [ ] Loading states đẹp hơn
- [ ] Empty states với hướng dẫn

**Error Handling:**
- [ ] Toast notifications cho success/error
- [ ] Retry mechanism cho failed commands
- [ ] Offline indicator rõ ràng

---

## 📝 Notes

- **Agent đã có sẵn**: WebCamService, PowerService, KeyLoggerService
- **Hub Events cần dùng**: Xem `HubEvents.cs` trong Shared
- **Reference UI**: DeviceManager.razor cho style consistency

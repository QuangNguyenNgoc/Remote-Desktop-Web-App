# Issue #19b: [Test] E2E Test - Process Management

### 🎯 Mục Tiêu

Test toàn bộ flow process management: List, Kill, Start

### ✅ Checklist

**Test List Processes:**
- [ ] Click "List Processes" trong DeviceControl
- [ ] Verify: Process list hiển thị đúng (có PID, Name, Memory)
- [ ] Verify: List được sort theo name
- [ ] Test: Refresh list → data cập nhật

**Test Kill Process:**
- [ ] Mở Notepad thủ công trên máy Agent
- [ ] Click "List Processes" → tìm Notepad trong list
- [ ] Click "Kill" button trên row Notepad
- [ ] Verify: Notepad bị đóng
- [ ] Verify: UI update (Notepad không còn trong list)
- [ ] Test error: Kill process không tồn tại → show error message

**Test Start Process:**
- [ ] Nhập "notepad" vào input
- [ ] Click "Start Process"
- [ ] Verify: Notepad mở trên máy Agent
- [ ] Test error: Start process không tồn tại → show error message

**Performance:**
- [ ] List 100+ processes → verify không lag UI
- [ ] Kill/Start liên tiếp 5 lần → verify ổn định

### 🔗 Dependencies

- ✅ #19: DeviceControl UI có process management UI
- ✅ #18: Agent connected
- ✅ #15: ProcessService

### 📝 Notes

- Process list nên có pagination nếu > 50 items (optional)
- Consider: Add search/filter cho process list

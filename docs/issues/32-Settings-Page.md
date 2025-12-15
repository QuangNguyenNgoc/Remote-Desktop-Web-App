# Issue #32: [FE] Settings Page & Configuration

### 🎯 Mục Tiêu

Tạo trang Settings để cấu hình ứng dụng

### ✅ Checklist

**Settings Page UI:**
- [ ] Tạo `Settings.razor` page tại route `/settings`
- [ ] Sidebar navigation hoặc tabs
- [ ] Save/Cancel buttons

**General Settings:**
- [ ] Theme toggle (Light/Dark)
- [ ] Language selection (VI/EN)
- [ ] Notification preferences

**Security Settings:**
- [ ] Change passkey
- [ ] Session timeout duration
- [ ] View active sessions

**Agent Default Settings:**
- [ ] Default screenshot quality
- [ ] Command timeout
- [ ] Auto-reconnect interval

**About Section:**
- [ ] Version info
- [ ] License
- [ ] GitHub link

### 🔗 Dependencies

- ⏳ Làm sau MVP core

### 📝 Design Notes

- Lưu settings trong localStorage hoặc server-side
- Áp dụng theme ngay lập tức (không cần reload)

# Issue #25: [FE] Login Page UI

### 🎯 Mục Tiêu

Tạo trang đăng nhập đẹp và responsive

### ✅ Checklist

**UI Components:**
- [ ] Tạo `Login.razor` page tại route `/login`
- [ ] Form với input passkey (type="password")
- [ ] Logo/Brand của ứng dụng
- [ ] Button "Login" với loading state
- [ ] Error message khi passkey sai
- [ ] "Remember me" checkbox (lưu 7 ngày)

**Styling:**
- [ ] Gradient background hoặc dark theme
- [ ] Centered card layout
- [ ] Responsive cho mobile/tablet
- [ ] Animation khi submit

**Logic:**
- [ ] Submit passkey đến API `/api/auth/verify`
- [ ] Redirect đến `/devices` sau khi thành công
- [ ] Hiển thị error toast nếu fail

### 🔗 Dependencies

- ⏳ #24: Passkey authentication backend

### 📝 Design Reference

```
┌────────────────────────────────────────┐
│                                        │
│            🖥️ Remote Control           │
│                                        │
│     ┌──────────────────────────┐       │
│     │  Enter Passkey           │       │
│     │  ●●●●●●●●                │       │
│     └──────────────────────────┘       │
│                                        │
│     [  ] Remember me                   │
│                                        │
│     ┌──────────────────────────┐       │
│     │        LOGIN             │       │
│     └──────────────────────────┘       │
│                                        │
└────────────────────────────────────────┘
```

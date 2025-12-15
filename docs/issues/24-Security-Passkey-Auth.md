# Issue #24: [Security] Implement Simple Passkey Authentication

### 🎯 Mục Tiêu

Thêm authentication đơn giản với passkey để bảo vệ Dashboard và Hub

### ✅ Checklist

**Backend:**
- [ ] Thêm `PasskeyMiddleware` kiểm tra header/cookie `X-Passkey`
- [ ] Cấu hình passkey trong `appsettings.json`
- [ ] Bảo vệ `/devices`, `/devices/*` endpoints
- [ ] Bảo vệ SignalR hub connection

**Frontend:**
- [ ] Tạo `Login.razor` page với form nhập passkey
- [ ] Lưu passkey vào localStorage/cookie sau khi đăng nhập
- [ ] Redirect về login nếu chưa authenticated
- [ ] Thêm nút Logout

**Agent:**
- [ ] Đọc passkey từ config file
- [ ] Gửi passkey trong SignalR connection headers

### 🔗 Dependencies

- ⏳ Làm sau khi MVP core hoàn thành (#18, #19)

### 📝 Notes

- Đây là authentication đơn giản cho demo/lab
- Cho production cần upgrade lên JWT (xem 10-SECURITY-GUIDE.md)
- Passkey là 1 string secret, ví dụ: `"my-secret-2024"`

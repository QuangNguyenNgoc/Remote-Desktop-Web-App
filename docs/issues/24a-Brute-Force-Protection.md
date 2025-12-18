# Issue #24a: [Security] Brute-force Protection for Passkey

### 🎯 Mục Tiêu

Chống brute-force attack bằng cách block IP sau nhiều lần nhập sai passkey

### ✅ Checklist

**Backend:**
- [ ] Track failed login attempts per IP (dùng `MemoryCache`)
- [ ] Block IP sau 5 failed attempts trong 15 phút
- [ ] Trả về HTTP 429 Too Many Requests khi bị block

**Frontend (optional):**
- [ ] Hiển thị thông báo "Too many attempts. Try again later."

### 📝 Notes

- Scope hiện tại (`/devices`, `/remotehub`) đã hợp lý
- Dashboard, Activity logs chỉ view-only nên không cần bảo vệ thêm

### 🔗 Dependencies

- ⏳ #24: Passkey authentication (done)

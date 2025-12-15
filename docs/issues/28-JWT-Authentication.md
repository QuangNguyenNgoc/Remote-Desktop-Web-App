# Issue #28: [Security] JWT Authentication (Production)

### 🎯 Mục Tiêu

Nâng cấp từ Passkey lên JWT cho production deployment

### ✅ Checklist

**Backend Setup:**
- [ ] Add NuGet: `Microsoft.AspNetCore.Authentication.JwtBearer`
- [ ] Cấu hình JWT trong `Program.cs`
- [ ] Tạo `AuthController` với endpoints:
  - `POST /api/auth/agent-token` - Agent lấy token
  - `POST /api/auth/admin-token` - Admin lấy token
- [ ] Lưu JWT key trong environment variable

**Hub Protection:**
- [ ] Add `[Authorize]` attribute cho Hub
- [ ] Configure SignalR để đọc token từ query string
- [ ] Implement role-based authorization (Agent, Operator, Admin)

**Agent Update:**
- [ ] Lấy token từ auth endpoint
- [ ] Gửi token trong SignalR connection

**Frontend Update:**
- [ ] Cập nhật Login page cho username/password
- [ ] Lưu JWT token
- [ ] Gửi token trong SignalR connection

### 🔗 Dependencies

- ⏳ #24, #25: Basic auth hoàn thành trước
- Reference: `docs/10-SECURITY-GUIDE.md`

### 📝 Notes

- JWT expiry: 1 giờ
- Implement refresh token nếu cần
- KHÔNG commit secret key vào git!

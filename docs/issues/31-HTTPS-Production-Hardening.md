# Issue #31: [Security] HTTPS & Production Hardening

### 🎯 Mục Tiêu

Bảo mật ứng dụng cho production deployment

### ✅ Checklist

**HTTPS Configuration:**
- [ ] Generate SSL certificate (Let's Encrypt hoặc self-signed)
- [ ] Configure Kestrel để sử dụng HTTPS
- [ ] Redirect HTTP → HTTPS
- [ ] HSTS header

**Security Headers:**
- [ ] Add security headers middleware
- [ ] X-Content-Type-Options: nosniff
- [ ] X-Frame-Options: SAMEORIGIN
- [ ] X-XSS-Protection: 1; mode=block
- [ ] Content-Security-Policy

**Rate Limiting:**
- [ ] Add rate limiting middleware
- [ ] Limit auth attempts (5/minute)
- [ ] Limit API calls (100/minute)

**Logging & Audit:**
- [ ] Log tất cả authentication attempts
- [ ] Log commands với user info
- [ ] Rotate logs định kỳ

**Input Validation:**
- [ ] Validate tất cả input từ client
- [ ] Sanitize data trước khi log
- [ ] Prevent SQL injection (nếu có DB)

### 🔗 Dependencies

- ⏳ #28: JWT Authentication
- Deploy lên VPS/cloud

### 📝 Notes

- Reference: `docs/10-SECURITY-GUIDE.md` section 4
- OWASP Top 10 compliance

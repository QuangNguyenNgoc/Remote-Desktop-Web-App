# Issue #24a: [Security] Brute-force Protection for Passkey

### 🎯 Mục Tiêu

Chống brute-force attack bằng cách block IP sau nhiều lần nhập sai passkey

### ✅ Checklist

**Backend:**
- [ ] Tạo `RateLimitService` hoặc sử dụng `MemoryCache`
- [ ] Track failed login attempts per IP
- [ ] Block IP sau 5 failed attempts trong 15 phút
- [ ] Trả về HTTP 429 Too Many Requests khi bị block

**Frontend:**
- [ ] Hiển thị thông báo "Too many attempts. Try again later."
- [ ] Countdown timer (optional)

### 📝 Implementation Notes

```csharp
// Trong AuthController hoặc PasskeyMiddleware
private static readonly ConcurrentDictionary<string, (int Count, DateTime BlockedUntil)> _attempts = new();

// Khi login failed:
var ip = context.Connection.RemoteIpAddress?.ToString();
if (_attempts.TryGetValue(ip, out var record))
{
    if (DateTime.UtcNow < record.BlockedUntil)
    {
        // Return 429
    }
    if (record.Count >= 5)
    {
        // Block for 15 minutes
    }
}
```

### 🔗 Dependencies

- ⏳ #24: Passkey authentication (done)

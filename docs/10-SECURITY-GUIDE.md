# Security & Authentication Guide - Remote Control Desktop

> ⚠️ **Trạng thái**: PLANNED - Chưa implement trong MVP  
> **Mục đích file này**: Tài liệu học tập và reference cho development sau này

---

## 1. Tổng Quan Vấn Đề Bảo Mật

### 1.1 Tại Sao Cần Authentication?

**Hiện tại (Development/Demo):**
```
┌─────────────────┐         ┌─────────────────┐
│   Any Computer  │ ──────► │    Hub Server   │  ← Ai cũng connect được!
└─────────────────┘         └─────────────────┘
```

**Rủi ro:**
- ❌ Bất kỳ ai biết URL hub có thể giả làm Agent
- ❌ Bất kỳ ai có thể mở Dashboard và điều khiển máy khác
- ❌ Không có audit log ai đã làm gì

**Sau khi implement Auth:**
```
┌─────────────────┐         ┌─────────────────┐         ┌─────────────────┐
│   Agent/Admin   │ ──JWT─► │    Hub Server   │ ─verify─►│   Auth Server   │
└─────────────────┘         └─────────────────┘         └─────────────────┘
                                     │
                                     ▼
                            ✅ Chỉ authorized clients
```

### 1.2 Khi Nào PHẢI Implement?

| Scenario | Cần Auth? | Lý do |
|----------|-----------|-------|
| Demo trong phòng lab | ❌ Không | Mạng closed, trusted |
| Nộp bài (localhost) | ❌ Không | Chỉ 1 máy |
| LAN party với bạn bè | ⚠️ Tùy | Có thể bị troll |
| Deploy lên VPS/Internet | ✅ BẮT BUỘC | Public access |
| Production real-world | ✅ BẮT BUỘC + thêm RBAC | Legal requirements |

---

## 2. JWT Authentication - Chi Tiết

### 2.1 JWT Là Gì?

**JSON Web Token** - Một chuỗi string chứa thông tin user/agent đã được mã hóa và ký.

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhZ2VudElkIjoiYWJjMTIzIiwicm9sZSI6ImFnZW50Iiwi
ZXhwIjoxNzAxNjAwMDAwfQ.signature_here
```

**Cấu trúc:**
```
HEADER.PAYLOAD.SIGNATURE
  │       │         │
  │       │         └── Chữ ký để verify (dùng secret key)
  │       └── Dữ liệu: { "agentId": "abc", "role": "agent", "exp": 123456 }
  └── Thuật toán: { "alg": "HS256", "typ": "JWT" }
```

### 2.2 Flow Authentication

```
┌─────────────────────────────────────────────────────────────────────┐
│                        AUTHENTICATION FLOW                          │
└─────────────────────────────────────────────────────────────────────┘

STEP 1: Agent/Admin lấy Token
═══════════════════════════════════════════════════════════════════════

Agent                           Auth Endpoint                    Database
  │                                   │                              │
  │ POST /api/auth/token              │                              │
  │ { "agentKey": "secret123" }       │                              │
  │ ────────────────────────────────► │                              │
  │                                   │  Verify agentKey             │
  │                                   │ ────────────────────────────►│
  │                                   │                              │
  │                                   │  ✅ Valid                    │
  │                                   │ ◄────────────────────────────│
  │                                   │                              │
  │  { "token": "eyJhbGc...",         │                              │
  │    "expiresIn": 3600 }            │                              │
  │ ◄──────────────────────────────── │                              │
  │                                   │                              │


STEP 2: Agent connect SignalR với Token
═══════════════════════════════════════════════════════════════════════

Agent                              SignalR Hub                    Auth Middleware
  │                                     │                              │
  │ Connect with Authorization header   │                              │
  │ "Bearer eyJhbGc..."                 │                              │
  │ ────────────────────────────────────►                              │
  │                                     │  Validate token              │
  │                                     │ ────────────────────────────►│
  │                                     │                              │
  │                                     │  ✅ Valid, extract claims    │
  │                                     │ ◄────────────────────────────│
  │                                     │                              │
  │  Connection established             │                              │
  │  Context.User = authenticated       │                              │
  │ ◄──────────────────────────────────│                              │
```

### 2.3 Code Implementation (Reference)

#### A. NuGet Packages Cần Thiết

```xml
<!-- RemoteControl.Web.csproj -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
```

#### B. Cấu Hình JWT trong Program.cs

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

        // SignalR cần config đặc biệt - token từ query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                
                if (!string.IsNullOrEmpty(accessToken) && 
                    path.StartsWithSegments("/remotehub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();  // Phải trước UseAuthorization
app.UseAuthorization();

app.MapHub<RemoteControlHub>("/remotehub");
```

#### C. appsettings.json

```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyAtLeast32Characters!",
    "Issuer": "RemoteControlServer",
    "Audience": "RemoteControlAgents",
    "ExpiryMinutes": 60
  }
}
```

> ⚠️ **QUAN TRỌNG**: Trong production, `Jwt:Key` phải được lưu trong:
> - Environment variables
> - Azure Key Vault
> - AWS Secrets Manager
> - Không bao giờ commit vào Git!

#### D. Auth Controller - Cấp Token

```csharp
// Controllers/AuthController.cs
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("agent-token")]
    public IActionResult GetAgentToken([FromBody] AgentAuthRequest request)
    {
        // TODO: Verify agentKey từ database
        // Ví dụ đơn giản: hardcode key (KHÔNG dùng trong production!)
        if (request.AgentKey != "demo-agent-key")
        {
            return Unauthorized(new { message = "Invalid agent key" });
        }

        var token = GenerateJwtToken(request.AgentId, "Agent");
        return Ok(new { token, expiresIn = 3600 });
    }

    [HttpPost("admin-token")]
    public IActionResult GetAdminToken([FromBody] AdminAuthRequest request)
    {
        // TODO: Verify username/password từ database
        if (request.Username != "admin" || request.Password != "admin123")
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var token = GenerateJwtToken(request.Username, "Admin");
        return Ok(new { token, expiresIn = 3600 });
    }

    private string GenerateJwtToken(string subject, string role)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, subject),
            new Claim(ClaimTypes.Role, role),
            new Claim("sub", subject)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60")),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// DTOs
public record AgentAuthRequest(string AgentId, string AgentKey);
public record AdminAuthRequest(string Username, string Password);
```

#### E. Bảo Vệ Hub với [Authorize]

```csharp
// Hubs/RemoteControlHub.cs
using Microsoft.AspNetCore.Authorization;

[Authorize]  // Toàn bộ hub cần authenticated
public class RemoteControlHub : Hub
{
    public async Task RegisterAgent(AgentInfo info)
    {
        // Lấy thông tin từ JWT claims
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

        if (role != "Agent")
        {
            throw new HubException("Only agents can register");
        }

        // info.AgentId phải match với token
        if (info.AgentId != userId)
        {
            throw new HubException("AgentId mismatch");
        }

        // Proceed with registration
        info.ConnectionId = Context.ConnectionId;
        // ...
    }

    [Authorize(Roles = "Admin")]  // Chỉ admin mới được gọi
    public async Task SendCommand(CommandRequest command)
    {
        // ...
    }
}
```

#### F. Agent Connect với Token

```csharp
// Agent/Program.cs
public class AgentConnection
{
    private string? _jwtToken;
    private HubConnection? _connection;

    public async Task ConnectAsync()
    {
        // Step 1: Lấy token từ auth endpoint
        _jwtToken = await GetTokenAsync();

        // Step 2: Connect SignalR với token
        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5048/remotehub", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_jwtToken);
            })
            .WithAutomaticReconnect()
            .Build();

        await _connection.StartAsync();
    }

    private async Task<string?> GetTokenAsync()
    {
        using var client = new HttpClient();
        var response = await client.PostAsJsonAsync(
            "http://localhost:5048/api/auth/agent-token",
            new { AgentId = _agentId, AgentKey = "demo-agent-key" });

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            return result?.Token;
        }
        return null;
    }
}

record TokenResponse(string Token, int ExpiresIn);
```

---

## 3. Authorization (Phân Quyền)

### 3.1 Role-Based Access Control (RBAC)

```csharp
public enum UserRole
{
    Agent,      // Chỉ được register và respond commands
    Operator,   // Xem dashboard, gửi commands cơ bản
    Admin       // Full access: kick agents, shutdown, etc.
}
```

### 3.2 Permission Matrix

| Action | Agent | Operator | Admin |
|--------|-------|----------|-------|
| Connect to Hub | ✅ | ✅ | ✅ |
| Register as Agent | ✅ | ❌ | ❌ |
| View Dashboard | ❌ | ✅ | ✅ |
| List Processes | ❌ | ✅ | ✅ |
| Kill Process | ❌ | ✅ | ✅ |
| Capture Screenshot | ❌ | ✅ | ✅ |
| Start Keylogger | ❌ | ❌ | ✅ |
| Shutdown Agent | ❌ | ❌ | ✅ |
| Kick Agent | ❌ | ❌ | ✅ |

### 3.3 Implementation với Policy

```csharp
// Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanSendBasicCommands", policy =>
        policy.RequireRole("Operator", "Admin"));

    options.AddPolicy("CanSendDangerousCommands", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("AgentOnly", policy =>
        policy.RequireRole("Agent"));
});

// Hub
[Authorize(Policy = "CanSendBasicCommands")]
public async Task SendCommand(CommandRequest command)
{
    // Check dangerous commands need higher permission
    if (command.Type is CommandType.Shutdown or 
        CommandType.StartKeylogger or 
        CommandType.Quit)
    {
        var isAdmin = Context.User?.IsInRole("Admin") ?? false;
        if (!isAdmin)
        {
            throw new HubException("Admin role required for this command");
        }
    }
    // ...
}
```

---

## 4. Các Tấn Công Phổ Biến & Cách Phòng Chống

### 4.1 Token Theft

**Tấn công:** Hacker đánh cắp JWT token từ memory/network

**Phòng chống:**
- ✅ Dùng HTTPS trong production
- ✅ Token expiry ngắn (1 giờ)
- ✅ Implement refresh token
- ✅ Không log token

### 4.2 Brute Force Auth Endpoint

**Tấn công:** Thử hàng nghìn password

**Phòng chống:**
```csharp
// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;  // Max 5 attempts per minute
    });
});

[EnableRateLimiting("auth")]
[HttpPost("admin-token")]
public IActionResult GetAdminToken(...) { }
```

### 4.3 Replay Attack

**Tấn công:** Capture và gửi lại command cũ

**Phòng chống:**
- ✅ Check `CommandRequest.RequestedAt` - reject nếu quá 30s
- ✅ Track `CommandId` đã xử lý - reject duplicate

---

## 5. Checklist Implementation

### Phase 1: Cơ Bản (MVP+)
- [ ] Thêm JWT authentication
- [ ] Protect Hub với `[Authorize]`
- [ ] Agent token endpoint
- [ ] Admin token endpoint

### Phase 2: Roles & Permissions
- [ ] Implement RBAC (Agent, Operator, Admin)
- [ ] Permission matrix
- [ ] Policy-based authorization

### Phase 3: Production Hardening
- [ ] HTTPS enforcement
- [ ] Rate limiting
- [ ] Refresh token
- [ ] Audit logging
- [ ] Secure secret storage (Key Vault)

---

## 6. Tài Liệu Tham Khảo

- [ASP.NET Core Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)
- [JWT.io - Debugger & Libraries](https://jwt.io/)
- [SignalR Authentication](https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)

---

**Cập Nhật**: 04/12/2024  
**Phiên Bản**: 1.0  
**Trạng Thái**: PLANNED - Reference cho future development

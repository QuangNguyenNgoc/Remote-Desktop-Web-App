# Login Features - Code Reference
> Từ branch issue #25, để áp dụng vào main UI

---

## 1. Native C# HttpClient API Call (thay vì JS interop)

```razor
@using System.Net.Http.Json
@inject HttpClient Http

// Trong LoginAsync():
var resp = await Http.PostAsJsonAsync(Nav.ToAbsoluteUri("/api/auth/verify"), new { Passkey = _passkey });

if (!resp.IsSuccessStatusCode)
{
    _error = "Login failed. Please try again.";
    return;
}

var data = await resp.Content.ReadFromJsonAsync<VerifyResponse>();
var ok = data?.Success == true;

// Response class
private sealed class VerifyResponse
{
    public bool Success { get; set; }
}
```

---

## 2. Remember Me (7 ngày) với localStorage + Expiry

### State variables
```csharp
private bool _rememberMe = false;
```

### HTML Checkbox
```html
<label class="remember">
    <input type="checkbox" @bind="_rememberMe" disabled="@_isLoading" />
    <span>Remember me</span>
    <span class="muted">(7 days)</span>
</label>
```

### Persist Login Function
```csharp
private async Task PersistLoginAsync(bool remember)
{
    if (remember)
    {
        var until = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds().ToString();
        await JS.InvokeVoidAsync("localStorage.setItem", "rc_auth_until", until);
        await JS.InvokeVoidAsync("localStorage.setItem", "rc_auth", "1");
        await JS.InvokeVoidAsync("sessionStorage.removeItem", "rc_auth_session");
    }
    else
    {
        await JS.InvokeVoidAsync("sessionStorage.setItem", "rc_auth_session", "1");
        await JS.InvokeVoidAsync("localStorage.removeItem", "rc_auth_until");
        await JS.InvokeVoidAsync("localStorage.removeItem", "rc_auth");
    }
}
```

### Gọi sau khi login thành công
```csharp
await PersistLoginAsync(_rememberMe);
Nav.NavigateTo("/devices", forceLoad: true);
```

---

## 3. Auto-login Check (khi page load)

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (!firstRender) return;

    // JS interop chỉ gọi an toàn sau khi render xong lần đầu
    if (await IsRememberedAndValidAsync())
    {
        Nav.NavigateTo("/devices", forceLoad: true);
    }
}

private async Task<bool> IsRememberedAndValidAsync()
{
    // Check session storage first (không remember)
    var session = await JS.InvokeAsync<string>("sessionStorage.getItem", "rc_auth_session");
    if (session == "1") return true;

    // Check localStorage với expiry
    var flag = await JS.InvokeAsync<string>("localStorage.getItem", "rc_auth");
    var untilStr = await JS.InvokeAsync<string>("localStorage.getItem", "rc_auth_until");
    if (flag != "1" || string.IsNullOrWhiteSpace(untilStr)) return false;

    if (!long.TryParse(untilStr, out var until)) return false;
    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    return now <= until;
}
```

---

## 4. Toast Notification

### State
```csharp
private bool _toastVisible = false;
private string _toastMessage = "";
```

### HTML
```html
@if (_toastVisible)
{
    <div class="toast" role="status" aria-live="polite">
        @_toastMessage
    </div>
}
```

### Function
```csharp
private async Task ShowToastAsync(string message)
{
    _toastMessage = message;
    _toastVisible = true;
    StateHasChanged();

    await Task.Delay(2500);

    _toastVisible = false;
    StateHasChanged();
}
```

### Gọi khi lỗi
```csharp
_error = "Invalid passkey.";
await ShowToastAsync("Invalid passkey.");
```

### CSS cho Toast
```css
.toast {
    position: fixed;
    bottom: 24px;
    left: 50%;
    transform: translateX(-50%);
    background: #ef4444;
    color: #fff;
    padding: 12px 24px;
    border-radius: 8px;
    font-size: 14px;
    box-shadow: 0 4px 12px rgba(0,0,0,0.3);
    z-index: 9999;
    animation: slideUp 0.3s ease;
}

@keyframes slideUp {
    from { opacity: 0; transform: translateX(-50%) translateY(20px); }
    to { opacity: 1; transform: translateX(-50%) translateY(0); }
}
```

---

## 5. AuthController Dual Routes

```csharp
[ApiController]
[Route("[controller]")]     // /Auth/verify (legacy)
[Route("api/auth")]         // /api/auth/verify (REST standard)
public class AuthController : ControllerBase
{
    private readonly string _expectedPasskey;

    public AuthController(IConfiguration config)
    {
        _expectedPasskey = config["Passkey:Value"] ?? "";
    }

    public record VerifyRequest(string Passkey);
    public record VerifyResponse(bool Success);

    [HttpPost("verify")]
    public ActionResult<VerifyResponse> Verify([FromBody] VerifyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Passkey))
            return Ok(new VerifyResponse(false));

        var isValid = string.Equals(request.Passkey, _expectedPasskey, StringComparison.Ordinal);
        return Ok(new VerifyResponse(isValid));
    }
}
```

---

## BONUS: Separate Dev Config

**appsettings.Development.json** (không commit secrets)
```json
{
  "Passkey": {
    "Value": "i-love-hcmus"
  }
}
```

**appsettings.json** (production, để trống hoặc placeholder)
```json
{
  "Passkey": {
    "Value": ""
  }
}
```

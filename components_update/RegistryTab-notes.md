# RegistryTab Component - Development Notes

## 📁 File Info
- **Skeleton:** `components_update/RegistryTab.razor`
- **Đích:** `RemoteControl.Web/Components/Pages/DeviceFeatures/RegistryTab.razor`

---

## ✅ Tính năng cốt lõi (đã implement ở Agent)

### Read Operations
| Service Method | CommandType | Description |
|----------------|-------------|-------------|
| `ListSubKeys()` | `ListRegistrySubKeys` | Lấy danh sách subkeys |
| `ListValues()` | `ListRegistryValues` | Lấy tất cả values |
| `ReadValue()` | `ReadRegistry` | Đọc một value cụ thể |
| `GetKeyInfo()` | `GetRegistryKeyInfo` | Lấy thông tin key |
| `KeyExists()` | (internal) | Kiểm tra key tồn tại |
| `ValueExists()` | (internal) | Kiểm tra value tồn tại |
| `GetRootKeys()` | (internal) | Lấy list root keys |

### Write Operations (cần Admin cho HKLM)
| Service Method | CommandType | Description |
|----------------|-------------|-------------|
| `WriteValue()` | `WriteRegistry` | Tạo/cập nhật value |
| `CreateKey()` | `CreateRegistryKey` | Tạo key mới |
| `DeleteKey()` | `DeleteRegistryKey` | Xóa key (hỗ trợ recursive) |
| `DeleteValue()` | `DeleteRegistryValue` | Xóa value |

---

## 🎮 UI Components - MAPPING ĐẦY ĐỦ

### Toolbar Buttons
| Button | Màu | Action | Service Method |
|--------|-----|--------|----------------|
| **Go** | Teal | Navigate đến path trong textbox | `ListSubKeys()` + `ListValues()` |
| **Refresh** | RoyalBlue | Reload current key | `ListValues()` |
| **+ Key** | MediumSeaGreen | Tạo subkey mới | `CreateKey()` |
| **+ Value** | DodgerBlue | Tạo value mới (REG_SZ) | `WriteValue()` |
| **Delete** | IndianRed | Xóa key/value đang chọn | `DeleteKey()` / `DeleteValue()` |
| **← Back** | Gray | History navigation | (client-side) |

### TreeView Context Menu (Right-click trên key)
| Menu Item | Action | Service Method |
|-----------|--------|----------------|
| Create Subkey | Tạo key con | `CreateKey()` |
| Delete Key | Xóa key + confirm | `DeleteKey(recursive: true)` |
| --- | separator | --- |
| Refresh | Reload subkeys | `ListSubKeys()` |
| Copy Path | Copy path to clipboard | (client-side) |

### ListView Context Menu (Right-click trên value)
| Menu Item | Action | Service Method |
|-----------|--------|----------------|
| Edit Value | Mở dialog edit | `WriteValue()` |
| Delete Value | Xóa value + confirm | `DeleteValue()` |
| --- | separator | --- |
| New String Value | Tạo REG_SZ | `WriteValue(type: "REG_SZ")` |
| New DWORD Value | Tạo REG_DWORD | `WriteValue(type: "REG_DWORD")` |
| New QWORD Value | Tạo REG_QWORD | `WriteValue(type: "REG_QWORD")` |
| --- | separator | --- |
| Copy Value Name | Copy name | (client-side) |
| Copy Value Data | Copy data | (client-side) |

### ListView Events
| Event | Action |
|-------|--------|
| **Double-click** | Edit selected value |
| **Select** | Highlight row |

---

## 📦 Dialogs cần tạo

### 1. Input Dialog (đã có ở Agent)
- Title, Prompt, Default value
- OK / Cancel buttons
- Dùng cho: Create key, Create value, Edit value

### 2. Confirm Dialog
- Message với warning icon
- Yes / No buttons
- Dùng cho: Delete key, Delete value

---

## 🎨 UI Layout

```
┌────────────────────────────────────────────────────────────────────┐
│ TOOLBAR                                                            │
│ [Path: ___________________] [Go] [Refresh] [+Key] [+Value] [Delete]│
├─────────────────────────────┬──────────────────────────────────────┤
│ TREE VIEW (280px)           │ VALUES LIST VIEW                      │
│                             │                                       │
│ ▶ HKEY_CLASSES_ROOT         │ Name          │ Type      │ Data      │
│ ▼ HKEY_CURRENT_USER         │ ─────────────────────────────────────│
│   ▶ Software                │ (Default)     │ REG_SZ    │ (value)   │
│     ▶ Microsoft             │ Setting1      │ REG_DWORD │ 123       │
│       ▶ Notepad             │ Setting2      │ REG_SZ    │ hello     │
│ ▶ HKEY_LOCAL_MACHINE        │                                       │
│                             │ [Right-click for context menu]        │
├─────────────────────────────┴──────────────────────────────────────┤
│ STATUS BAR: Loaded 5 values from HKCU\Software\Microsoft\Notepad   │
└────────────────────────────────────────────────────────────────────┘
```

---

## ⚠️ Lưu ý quan trọng

### Security
1. **HKLM cần Admin** - Hiện warning: `"Lỗi: Không có quyền truy cập. Cần chạy với quyền Administrator."`
2. **Confirm trước khi delete** - MessageBox với YesNo
3. **Recursive delete** - Xóa key sẽ xóa tất cả subkeys và values

### Performance
1. **Lazy loading** - Chỉ load subkeys khi expand
2. **Truncate long data** - `data.Length > 200 ? data.Substring(0, 200) + "..." : data`

### Value Types
| Type | Input | Notes |
|------|-------|-------|
| REG_SZ | String | Plain text |
| REG_DWORD | Number | 32-bit integer (0 - 4294967295) |
| REG_QWORD | Number | 64-bit integer |
| REG_BINARY | Hex | "AA BB CC" hoặc "AABBCC" |
| REG_MULTI_SZ | Multi-line | Separated by \0 hoặc \n |
| REG_EXPAND_SZ | String | Contains %VARIABLE% |

---

## 🔗 SignalR Integration (cho Web)

### Send Command
```csharp
var request = new CommandRequest
{
    AgentId = AgentId,
    Type = CommandType.ListRegistrySubKeys,
    RegistryKeyPath = "HKEY_CURRENT_USER\\Software"
};
await Hub.SendAsync("SendCommand", request);
```

### Receive Response
```csharp
Hub.On<CommandResult>("ReceiveCommandResult", result =>
{
    switch (result.Data)
    {
        case RegistrySubKeysResult subKeys:
            // Update tree
            break;
        case RegistryValuesResult values:
            // Update list
            break;
        case RegistryResult regResult:
            // Show status
            break;
    }
});
```

---

## 🔜 Future Features (chưa implement)

| Issue | Feature | Priority |
|-------|---------|----------|
| #1 | Export to .reg file | Medium |
| #2 | Import from .reg file | Medium |
| #3 | Search keys/values | High |
| #4 | Rename key/value | Low |

---

## 📌 Test Keys an toàn

| Path | Mô tả |
|------|-------|
| `HKCU\Software\Microsoft\Notepad` | Settings của Notepad |
| `HKCU\Environment` | Environment variables của user |
| `HKCU\Console` | Console settings |
| `HKCU\Software\RemoteControlTest` | Test key (tự tạo) |
| `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion` | Windows info (read-only) |

---

## ✅ Checklist trước khi merge

### Agent (đã xong)
- [x] RegistryService với CRUD methods
- [x] CommandHandler với 8 registry handlers
- [x] AgentDebugForm với full UI (toolbar, context menus, dialogs)

### Web (bạn làm)
- [ ] RegistryTab.razor layout
- [ ] TreeView component
- [ ] ListView component
- [ ] Toolbar buttons
- [ ] Context menus
- [ ] Input/Confirm dialogs
- [ ] SignalR integration
- [ ] Loading states
- [ ] Error handling với toast

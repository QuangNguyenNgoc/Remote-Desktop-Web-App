# Registry Service - Test Guide & Issues

## ✅ Verification Status

### Shared Models (Complete)
| Model | File | Status |
|-------|------|--------|
| `RegistryResult` | CommandResult.cs | ✅ |
| `RegistrySubKeysResult` | CommandResult.cs | ✅ |
| `RegistryValuesResult` | CommandResult.cs | ✅ |
| `RegistryValueInfo` | CommandResult.cs | ✅ |
| `RegistryKeyInfoResult` | CommandResult.cs | ✅ |

### CommandType (Complete)
- `ReadRegistry`, `WriteRegistry`, `CreateRegistryKey`, `DeleteRegistryKey`, `DeleteRegistryValue`
- `ListRegistrySubKeys`, `ListRegistryValues`, `GetRegistryKeyInfo`

### Agent Side (Complete)
| Component | Status |
|-----------|--------|
| `RegistryService.cs` | ✅ 11 methods |
| `CommandHandler.cs` | ✅ 8 handlers |
| `AgentDebugForm.cs` | ✅ Registry Tab |

### Web Side (NOT Implemented)
❌ Chưa có component Registry cho Web

---

## 🧪 Test trực tiếp trên Agent Console

### Cách 1: Dùng AgentDebugForm UI
1. Run Agent: `dotnet run`
2. Chọn tab **"Registry"**
3. Tương tác:
   - Click expand tree để xem subkeys
   - Click key để xem values
   - Nhập path vào textbox → Click "Go"

### Cách 2: Test trực tiếp RegistryService (code test)
Thêm đoạn code sau vào `Program.cs` của Agent để test:

```csharp
// Test RegistryService
var regService = new RegistryService();

// Test 1: List SubKeys
Console.WriteLine("=== Test ListSubKeys ===");
var subKeys = regService.ListSubKeys("HKEY_CURRENT_USER\\Software");
Console.WriteLine($"Found {subKeys.SubKeys.Count} subkeys");
subKeys.SubKeys.Take(5).ToList().ForEach(k => Console.WriteLine($"  - {k}"));

// Test 2: List Values
Console.WriteLine("\n=== Test ListValues ===");
var values = regService.ListValues("HKEY_CURRENT_USER\\Environment");
Console.WriteLine($"Found {values.Values.Count} values");
values.Values.ForEach(v => Console.WriteLine($"  {v.Name} ({v.Type}) = {v.Data}"));

// Test 3: Read specific value
Console.WriteLine("\n=== Test ReadValue ===");
var readResult = regService.ReadValue("HKEY_CURRENT_USER\\Environment", "Path");
Console.WriteLine($"Path = {readResult.Value}");

// Test 4: Create/Write/Delete (careful!)
Console.WriteLine("\n=== Test Create/Write/Delete ===");
var testKeyPath = "HKEY_CURRENT_USER\\Software\\RemoteControlTest";

// Create key
var createResult = regService.CreateKey(testKeyPath);
Console.WriteLine($"CreateKey: {createResult.OperationMessage}");

// Write value
var writeResult = regService.WriteValue(testKeyPath, "TestValue", "Hello World", "REG_SZ");
Console.WriteLine($"WriteValue: {writeResult.OperationMessage}");

// Read back
var verifyResult = regService.ReadValue(testKeyPath, "TestValue");
Console.WriteLine($"Verify: {verifyResult.Value}");

// Delete value
var delValResult = regService.DeleteValue(testKeyPath, "TestValue");
Console.WriteLine($"DeleteValue: {delValResult.OperationMessage}");

// Delete key
var delKeyResult = regService.DeleteKey(testKeyPath);
Console.WriteLine($"DeleteKey: {delKeyResult.OperationMessage}");

Console.WriteLine("\n=== All tests passed! ===");
```

### Cách 3: Test qua CommandHandler (simulate Web request)
```csharp
var handler = new CommandHandler();

// Test ListRegistrySubKeys
var request = new CommandRequest
{
    Type = CommandType.ListRegistrySubKeys,
    RegistryKeyPath = "HKEY_CURRENT_USER\\Software"
};
var result = handler.HandleCommand(request);
Console.WriteLine($"Success: {result.Success}, Message: {result.Message}");
if (result.Data is RegistrySubKeysResult data)
{
    Console.WriteLine($"SubKeys: {string.Join(", ", data.SubKeys.Take(5))}...");
}
```

---

## 📋 Future Feature Issues

### Issue #1: Export Registry Key to .reg file
**Title:** `[Feature] Export Registry Key to .reg file`

**Description:**
Thêm method `ExportKey(string keyPath, string outputPath)` để xuất registry key ra file `.reg`.

**Acceptance Criteria:**
- [ ] Export single key với tất cả values
- [ ] Export recursive (include subkeys)
- [ ] Output đúng format `.reg` chuẩn Windows
- [ ] Handle errors (access denied, key not found)

**Files to modify:**
- `RegistryService.cs` - add `ExportKey()` method
- `CommandResult.cs` - add `RegistryExportResult` if needed
- `CommandRequest.cs` - add `ExportRegistry` CommandType
- `CommandHandler.cs` - add handler

---

### Issue #2: Import Registry from .reg file
**Title:** `[Feature] Import Registry from .reg file`

**Description:**
Thêm method `ImportKey(string regFilePath)` để import từ file `.reg`.

**Acceptance Criteria:**
- [ ] Parse `.reg` file format
- [ ] Create keys và values theo file
- [ ] Handle errors (invalid format, access denied)
- [ ] Warning/confirmation trước khi import

**Security Note:** ⚠️ Cần cẩn thận - import có thể gây hại hệ thống

**Files to modify:**
- `RegistryService.cs` - add `ImportKey()` method
- `CommandRequest.cs` - add `ImportRegistry` CommandType
- `CommandHandler.cs` - add handler

---

### Issue #3: Search Registry
**Title:** `[Feature] Search Registry Keys/Values`

**Description:**
Thêm method `SearchRegistry(string keyPath, string query, bool searchKeys, bool searchValues, bool recursive)`.

**Acceptance Criteria:**
- [ ] Search key names matching query
- [ ] Search value names matching query
- [ ] Search value data matching query
- [ ] Recursive search option
- [ ] Limit results (max 100)

**Files to modify:**
- `RegistryService.cs` - add `SearchRegistry()` method
- `CommandResult.cs` - add `RegistrySearchResult`
- `CommandRequest.cs` - add `SearchRegistry` CommandType
- `CommandHandler.cs` - add handler

---

## 🔜 Next Steps for Web UI

Khi merge xong branch này về main, cần implement:

1. **RegistryTab.razor** - Component hiển thị registry browser
2. **SignalR integration** - Gọi các CommandType registry qua hub
3. **TreeView component** - Hiển thị cây registry
4. **ListView component** - Hiển thị values

**Reference:** Xem `AgentDebugForm.cs` lines 190-397 cho UI inspiration.

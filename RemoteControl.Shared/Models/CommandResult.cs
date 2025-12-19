namespace RemoteControl.Shared.Models;

/// <summary>
/// Represents the result of a command execution from Agent to Web
/// </summary>
public class CommandResult
{
    /// <summary>
    /// Matches the CommandRequest.CommandId
    /// </summary>
    public string CommandId { get; set; } = string.Empty;

    /// <summary>
    /// Agent that executed the command
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// Whether the command executed successfully
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Status or error message
    /// Examples from protocol: "Đã diệt chương trình", "Lỗi", "Set value thành công"
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Flexible data payload (can be ProcessListResult, ScreenshotResult, KeylogResult, etc.)
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// When the command completed
    /// </summary>
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

// ===== Specialized Result Types =====

/// <summary>
/// Result for ListProcesses or ListApplications commands
/// Based on socket protocol: ProcessName, Id, ThreadCount
/// </summary>
public class ProcessListResult
{
    public List<ProcessInfo> Processes { get; set; } = new();
}

/// <summary>
/// Information about a single process
/// Based on socket protocol response format
/// </summary>
public class ProcessInfo
{
    /// <summary>
    /// Process ID
    /// </summary>
    public int ProcessId { get; set; }

    /// <summary>
    /// Process name
    /// </summary>
    public string ProcessName { get; set; } = string.Empty;

    /// <summary>
    /// Window title (for applications)
    /// </summary>
    public string? WindowTitle { get; set; }

    /// <summary>
    /// Memory usage in megabytes
    /// </summary>
    public long MemoryUsageMB { get; set; }

    /// <summary>
    /// CPU usage percentage
    /// </summary>
    public double CpuUsage { get; set; }

    /// <summary>
    /// Thread count (from protocol)
    /// </summary>
    public int ThreadCount { get; set; }
}

/// <summary>
/// Result for CaptureScreen command
/// Based on socket protocol: Bitmap sent as byte array
/// </summary>
public class ScreenshotResult
{
    /// <summary>
    /// Image data as base64 string
    /// </summary>
    public string ImageBase64 { get; set; } = string.Empty;

    /// <summary>
    /// Image width in pixels
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Image height in pixels
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// When the screenshot was captured
    /// </summary>
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Image format (e.g., "bmp", "png", "jpg")
    /// </summary>
    public string Format { get; set; } = "bmp";
}

/// <summary>
/// Result for GetKeylogData command
/// Based on socket protocol: PRINT command returns text data
/// </summary>
public class KeylogResult
{
    public List<KeylogEntry> Entries { get; set; } = new();
}

/// <summary>
/// Single keylog entry
/// </summary>
public class KeylogEntry
{
    /// <summary>
    /// When the key was pressed
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Active window title when key was pressed
    /// </summary>
    public string WindowTitle { get; set; } = string.Empty;

    /// <summary>
    /// The key that was pressed
    /// </summary>
    public string KeyPressed { get; set; } = string.Empty;
}

/// <summary>
/// Result for registry operations
/// Based on socket protocol: Get/Set value responses
/// </summary>
public class RegistryResult
{
    /// <summary>
    /// Registry key path
    /// </summary>
    public string KeyPath { get; set; } = string.Empty;

    /// <summary>
    /// Value name
    /// </summary>
    public string? ValueName { get; set; }

    /// <summary>
    /// Value data
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Value type (String, Binary, DWORD, QWORD, Multi-String, Expandable String)
    /// </summary>
    public string? ValueType { get; set; }

    /// <summary>
    /// Operation success message from protocol
    /// Examples: "Tạo key thành công", "Set value thành công", "Lỗi"
    /// </summary>
    public string OperationMessage { get; set; } = string.Empty;
}

/// <summary>
/// Result for ListRegistrySubKeys command
/// Trả về danh sách các subkeys trong một registry key
/// </summary>
public class RegistrySubKeysResult
{
    /// <summary>
    /// Registry key path được query
    /// </summary>
    public string KeyPath { get; set; } = string.Empty;

    /// <summary>
    /// Danh sách tên các subkeys
    /// </summary>
    public List<string> SubKeys { get; set; } = new();
}

/// <summary>
/// Result for ListRegistryValues command
/// Trả về danh sách tất cả values trong một registry key
/// </summary>
public class RegistryValuesResult
{
    /// <summary>
    /// Registry key path được query
    /// </summary>
    public string KeyPath { get; set; } = string.Empty;

    /// <summary>
    /// Danh sách information của các values
    /// </summary>
    public List<RegistryValueInfo> Values { get; set; } = new();
}

/// <summary>
/// Information about a single registry value
/// Chi tiết về một registry value bao gồm tên, loại, và dữ liệu
/// </summary>
public class RegistryValueInfo
{
    /// <summary>
    /// Tên value (hoặc "(Default)" cho default value)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Loại value: REG_SZ, REG_DWORD, REG_QWORD, REG_BINARY, REG_MULTI_SZ, REG_EXPAND_SZ
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Dữ liệu của value (converted to string for display)
    /// </summary>
    public string Data { get; set; } = string.Empty;
}

/// <summary>
/// Result for GetRegistryKeyInfo command
/// Thông tin chi tiết về một registry key
/// </summary>
public class RegistryKeyInfoResult
{
    /// <summary>
    /// Registry key path
    /// </summary>
    public string KeyPath { get; set; } = string.Empty;

    /// <summary>
    /// Số lượng subkeys
    /// </summary>
    public int SubKeyCount { get; set; }

    /// <summary>
    /// Số lượng values
    /// </summary>
    public int ValueCount { get; set; }

    /// <summary>
    /// Thời gian chỉnh sửa cuối (nếu có)
    /// </summary>
    public DateTime? LastWriteTime { get; set; }

    /// <summary>
    /// Key có tồn tại hay không
    /// </summary>
    public bool Exists { get; set; }
}

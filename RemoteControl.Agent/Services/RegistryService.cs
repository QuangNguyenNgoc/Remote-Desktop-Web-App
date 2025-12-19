// RegistryService: Quản lý Windows Registry (Read/Write/Create/Delete)
// Sử dụng Microsoft.Win32.Registry API để truy cập Registry
// WARNING: Thao tác registry có thể ảnh hưởng đến hệ thống

using Microsoft.Win32;
using RemoteControl.Shared.Models;

namespace RemoteControl.Agent.Services;

public class RegistryService
{
    // ====== Các Root Keys được hỗ trợ ======
    private static readonly Dictionary<string, RegistryKey> RootKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        { "HKEY_CLASSES_ROOT", Registry.ClassesRoot },
        { "HKCR", Registry.ClassesRoot },
        { "HKEY_CURRENT_USER", Registry.CurrentUser },
        { "HKCU", Registry.CurrentUser },
        { "HKEY_LOCAL_MACHINE", Registry.LocalMachine },
        { "HKLM", Registry.LocalMachine },
        { "HKEY_USERS", Registry.Users },
        { "HKU", Registry.Users },
        { "HKEY_CURRENT_CONFIG", Registry.CurrentConfig },
        { "HKCC", Registry.CurrentConfig }
    };

    // ====== Mapping RegistryValueKind to readable string ======
    private static readonly Dictionary<RegistryValueKind, string> ValueKindNames = new()
    {
        { RegistryValueKind.String, "REG_SZ" },
        { RegistryValueKind.ExpandString, "REG_EXPAND_SZ" },
        { RegistryValueKind.Binary, "REG_BINARY" },
        { RegistryValueKind.DWord, "REG_DWORD" },
        { RegistryValueKind.MultiString, "REG_MULTI_SZ" },
        { RegistryValueKind.QWord, "REG_QWORD" },
        { RegistryValueKind.Unknown, "REG_UNKNOWN" },
        { RegistryValueKind.None, "REG_NONE" }
    };

    public RegistryService()
    {
        Console.WriteLine("[RegistryService] Initialized");
    }

    // ====== Parse Registry Path thành Root Key và SubKey ======
    private (RegistryKey? rootKey, string subKeyPath) ParseKeyPath(string fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
            return (null, string.Empty);

        // Tách root key và subkey path
        var parts = fullPath.Split(new[] { '\\' }, 2);
        var rootName = parts[0];
        var subKeyPath = parts.Length > 1 ? parts[1] : string.Empty;

        if (RootKeys.TryGetValue(rootName, out var rootKey))
            return (rootKey, subKeyPath);

        return (null, string.Empty);
    }

    // ====== Convert value to string for display ======
    private string ConvertValueToString(object? value, RegistryValueKind kind)
    {
        if (value == null) return "(null)";

        return kind switch
        {
            RegistryValueKind.Binary when value is byte[] bytes => BitConverter.ToString(bytes).Replace("-", " "),
            RegistryValueKind.MultiString when value is string[] strings => string.Join("\\0", strings),
            RegistryValueKind.DWord when value is int intVal => intVal.ToString(),
            RegistryValueKind.QWord when value is long longVal => longVal.ToString(),
            _ => value.ToString() ?? "(null)"
        };
    }

    // ====== Parse string to RegistryValueKind ======
    private RegistryValueKind ParseValueKind(string? typeStr)
    {
        if (string.IsNullOrWhiteSpace(typeStr))
            return RegistryValueKind.String;

        return typeStr.ToUpperInvariant() switch
        {
            "REG_SZ" or "STRING" => RegistryValueKind.String,
            "REG_EXPAND_SZ" or "EXPANDSTRING" => RegistryValueKind.ExpandString,
            "REG_BINARY" or "BINARY" => RegistryValueKind.Binary,
            "REG_DWORD" or "DWORD" => RegistryValueKind.DWord,
            "REG_MULTI_SZ" or "MULTISTRING" => RegistryValueKind.MultiString,
            "REG_QWORD" or "QWORD" => RegistryValueKind.QWord,
            _ => RegistryValueKind.String
        };
    }

    // ====== Parse string value to correct type based on kind ======
    private object? ParseValueData(string? data, RegistryValueKind kind)
    {
        if (data == null) return null;

        try
        {
            return kind switch
            {
                RegistryValueKind.DWord => int.TryParse(data, out var intVal) ? intVal : 0,
                RegistryValueKind.QWord => long.TryParse(data, out var longVal) ? longVal : 0L,
                RegistryValueKind.Binary => ParseHexString(data),
                RegistryValueKind.MultiString => data.Split(new[] { "\\0", "\n" }, StringSplitOptions.None),
                _ => data
            };
        }
        catch
        {
            return data;
        }
    }

    // ====== Parse hex string to byte array ======
    private byte[] ParseHexString(string hex)
    {
        // Remove spaces and convert "AA BB CC" or "AABBCC" to byte[]
        hex = hex.Replace(" ", "").Replace("-", "");
        if (hex.Length % 2 != 0)
            hex = "0" + hex;

        var bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return bytes;
    }

    // ====== Read Registry Value ======
    public RegistryResult ReadValue(string keyPath, string valueName)
    {
        var result = new RegistryResult { KeyPath = keyPath, ValueName = valueName };

        try
        {
            var (rootKey, subKeyPath) = ParseKeyPath(keyPath);
            if (rootKey == null)
            {
                result.OperationMessage = $"Lỗi: Không nhận diện được root key từ đường dẫn '{keyPath}'";
                return result;
            }

            using var key = rootKey.OpenSubKey(subKeyPath, false);
            if (key == null)
            {
                result.OperationMessage = $"Lỗi: Key không tồn tại '{keyPath}'";
                return result;
            }

            var value = key.GetValue(valueName);
            if (value == null)
            {
                result.OperationMessage = $"Lỗi: Value '{valueName}' không tồn tại";
                return result;
            }

            var kind = key.GetValueKind(valueName);
            result.Value = ConvertValueToString(value, kind);
            result.ValueType = ValueKindNames.GetValueOrDefault(kind, "REG_UNKNOWN");
            result.OperationMessage = "Đọc value thành công";
        }
        catch (Exception ex)
        {
            result.OperationMessage = $"Lỗi: {ex.Message}";
        }

        return result;
    }

    // ====== Write Registry Value ======
    public RegistryResult WriteValue(string keyPath, string valueName, string value, string? valueType)
    {
        var result = new RegistryResult { KeyPath = keyPath, ValueName = valueName, Value = value };

        try
        {
            var (rootKey, subKeyPath) = ParseKeyPath(keyPath);
            if (rootKey == null)
            {
                result.OperationMessage = $"Lỗi: Không nhận diện được root key từ đường dẫn '{keyPath}'";
                return result;
            }

            // Mở key với quyền write (tạo nếu chưa có)
            using var key = rootKey.OpenSubKey(subKeyPath, true) ?? rootKey.CreateSubKey(subKeyPath);
            if (key == null)
            {
                result.OperationMessage = $"Lỗi: Không thể mở hoặc tạo key '{keyPath}'";
                return result;
            }

            var kind = ParseValueKind(valueType);
            var parsedValue = ParseValueData(value, kind);

            key.SetValue(valueName, parsedValue ?? string.Empty, kind);

            result.ValueType = ValueKindNames.GetValueOrDefault(kind, "REG_SZ");
            result.OperationMessage = $"Set value '{valueName}' thành công";
        }
        catch (UnauthorizedAccessException)
        {
            result.OperationMessage = "Lỗi: Không có quyền truy cập. Cần chạy với quyền Administrator.";
        }
        catch (Exception ex)
        {
            result.OperationMessage = $"Lỗi: {ex.Message}";
        }

        return result;
    }

    // ====== Create Registry Key ======
    public RegistryResult CreateKey(string keyPath)
    {
        var result = new RegistryResult { KeyPath = keyPath };

        try
        {
            var (rootKey, subKeyPath) = ParseKeyPath(keyPath);
            if (rootKey == null)
            {
                result.OperationMessage = $"Lỗi: Không nhận diện được root key từ đường dẫn '{keyPath}'";
                return result;
            }

            using var key = rootKey.CreateSubKey(subKeyPath);
            if (key != null)
            {
                result.OperationMessage = $"Tạo key '{keyPath}' thành công";
            }
            else
            {
                result.OperationMessage = $"Lỗi: Không thể tạo key '{keyPath}'";
            }
        }
        catch (UnauthorizedAccessException)
        {
            result.OperationMessage = "Lỗi: Không có quyền truy cập. Cần chạy với quyền Administrator.";
        }
        catch (Exception ex)
        {
            result.OperationMessage = $"Lỗi: {ex.Message}";
        }

        return result;
    }

    // ====== Delete Registry Key ======
    public RegistryResult DeleteKey(string keyPath, bool recursive = false)
    {
        var result = new RegistryResult { KeyPath = keyPath };

        try
        {
            var (rootKey, subKeyPath) = ParseKeyPath(keyPath);
            if (rootKey == null)
            {
                result.OperationMessage = $"Lỗi: Không nhận diện được root key từ đường dẫn '{keyPath}'";
                return result;
            }

            if (recursive)
            {
                rootKey.DeleteSubKeyTree(subKeyPath, false);
            }
            else
            {
                rootKey.DeleteSubKey(subKeyPath, false);
            }

            result.OperationMessage = $"Xóa key '{keyPath}' thành công";
        }
        catch (UnauthorizedAccessException)
        {
            result.OperationMessage = "Lỗi: Không có quyền truy cập. Cần chạy với quyền Administrator.";
        }
        catch (Exception ex)
        {
            result.OperationMessage = $"Lỗi: {ex.Message}";
        }

        return result;
    }

    // ====== Delete Registry Value ======
    public RegistryResult DeleteValue(string keyPath, string valueName)
    {
        var result = new RegistryResult { KeyPath = keyPath, ValueName = valueName };

        try
        {
            var (rootKey, subKeyPath) = ParseKeyPath(keyPath);
            if (rootKey == null)
            {
                result.OperationMessage = $"Lỗi: Không nhận diện được root key từ đường dẫn '{keyPath}'";
                return result;
            }

            using var key = rootKey.OpenSubKey(subKeyPath, true);
            if (key == null)
            {
                result.OperationMessage = $"Lỗi: Key không tồn tại '{keyPath}'";
                return result;
            }

            key.DeleteValue(valueName, false);
            result.OperationMessage = $"Xóa value '{valueName}' thành công";
        }
        catch (UnauthorizedAccessException)
        {
            result.OperationMessage = "Lỗi: Không có quyền truy cập. Cần chạy với quyền Administrator.";
        }
        catch (Exception ex)
        {
            result.OperationMessage = $"Lỗi: {ex.Message}";
        }

        return result;
    }

    // ====== List SubKeys trong một Key ======
    public RegistrySubKeysResult ListSubKeys(string keyPath)
    {
        var result = new RegistrySubKeysResult { KeyPath = keyPath };

        try
        {
            var (rootKey, subKeyPath) = ParseKeyPath(keyPath);
            if (rootKey == null)
            {
                Console.WriteLine($"[RegistryService] Không nhận diện được root key: {keyPath}");
                return result;
            }

            using var key = string.IsNullOrEmpty(subKeyPath) ? rootKey : rootKey.OpenSubKey(subKeyPath, false);
            if (key == null)
            {
                Console.WriteLine($"[RegistryService] Key không tồn tại: {keyPath}");
                return result;
            }

            result.SubKeys = key.GetSubKeyNames().OrderBy(n => n).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RegistryService] ListSubKeys error: {ex.Message}");
        }

        return result;
    }

    // ====== List Values trong một Key ======
    public RegistryValuesResult ListValues(string keyPath)
    {
        var result = new RegistryValuesResult { KeyPath = keyPath };

        try
        {
            var (rootKey, subKeyPath) = ParseKeyPath(keyPath);
            if (rootKey == null)
            {
                Console.WriteLine($"[RegistryService] Không nhận diện được root key: {keyPath}");
                return result;
            }

            using var key = string.IsNullOrEmpty(subKeyPath) ? rootKey : rootKey.OpenSubKey(subKeyPath, false);
            if (key == null)
            {
                Console.WriteLine($"[RegistryService] Key không tồn tại: {keyPath}");
                return result;
            }

            var valueNames = key.GetValueNames();
            foreach (var name in valueNames.OrderBy(n => n))
            {
                try
                {
                    var value = key.GetValue(name);
                    var kind = key.GetValueKind(name);

                    result.Values.Add(new RegistryValueInfo
                    {
                        Name = string.IsNullOrEmpty(name) ? "(Default)" : name,
                        Type = ValueKindNames.GetValueOrDefault(kind, "REG_UNKNOWN"),
                        Data = ConvertValueToString(value, kind)
                    });
                }
                catch { /* Skip inaccessible values */ }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RegistryService] ListValues error: {ex.Message}");
        }

        return result;
    }

    // ====== Get Registry Key Info ======
    public RegistryKeyInfoResult GetKeyInfo(string keyPath)
    {
        var result = new RegistryKeyInfoResult { KeyPath = keyPath };

        try
        {
            var (rootKey, subKeyPath) = ParseKeyPath(keyPath);
            if (rootKey == null)
            {
                result.Exists = false;
                return result;
            }

            using var key = string.IsNullOrEmpty(subKeyPath) ? rootKey : rootKey.OpenSubKey(subKeyPath, false);
            if (key == null)
            {
                result.Exists = false;
                return result;
            }

            result.Exists = true;
            result.SubKeyCount = key.SubKeyCount;
            result.ValueCount = key.ValueCount;

            // Lấy last write time nếu có thể (Windows API)
            // Note: RegistryKey không có property LastWriteTime trực tiếp
            // Có thể dùng P/Invoke nhưng để đơn giản ta bỏ qua
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RegistryService] GetKeyInfo error: {ex.Message}");
            result.Exists = false;
        }

        return result;
    }

    // ====== Check if Key Exists ======
    public bool KeyExists(string keyPath)
    {
        try
        {
            var (rootKey, subKeyPath) = ParseKeyPath(keyPath);
            if (rootKey == null) return false;

            using var key = rootKey.OpenSubKey(subKeyPath, false);
            return key != null;
        }
        catch
        {
            return false;
        }
    }

    // ====== Check if Value Exists ======
    public bool ValueExists(string keyPath, string valueName)
    {
        try
        {
            var (rootKey, subKeyPath) = ParseKeyPath(keyPath);
            if (rootKey == null) return false;

            using var key = rootKey.OpenSubKey(subKeyPath, false);
            if (key == null) return false;

            return key.GetValue(valueName) != null;
        }
        catch
        {
            return false;
        }
    }

    // ====== Get All Root Keys ======
    public List<string> GetRootKeys()
    {
        return new List<string>
        {
            "HKEY_CLASSES_ROOT",
            "HKEY_CURRENT_USER",
            "HKEY_LOCAL_MACHINE",
            "HKEY_USERS",
            "HKEY_CURRENT_CONFIG"
        };
    }
}

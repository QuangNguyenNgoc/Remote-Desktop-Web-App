namespace RemoteControl.Shared.Models;

/// <summary>
/// Represents a command request from Web (Server) to Agent (Client)
/// Based on socket protocol: APPLICATION, SHUTDOWN, REGISTRY, TAKEPIC, PROCESS, KEYLOG, QUIT
/// </summary>
public class CommandRequest
{
    /// <summary>
    /// Mã định danh duy nhất cho mỗi command
    /// </summary>
    public string CommandId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Id của agent (máy) nhận lệnh
    /// NOTE: đây mới là field gốc, DeviceId bên dưới chỉ là alias.
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// Loại lệnh cần thực thi (CaptureScreen, ListProcesses, ...)
    /// </summary>
    public CommandType Type { get; set; }

    /// <summary>
    /// Các tham số bổ sung dạng key–value (linh hoạt cho nhiều loại lệnh)
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; } = new();

    /// <summary>
    /// Thời điểm tạo request
    /// </summary>
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    #region Convenience properties cho FE (DeviceControl UI)

    /// <summary>
    /// Alias cho AgentId để FE dùng cho dễ hiểu.
    /// DeviceId <=> AgentId
    /// </summary>
    public string DeviceId
    {
        get => AgentId;
        set => AgentId = value;
    }

    /// <summary>
    /// Pid của process, lưu/đọc từ Parameters["ProcessId"]
    /// </summary>
    public int? ProcessId
    {
        get
        {
            if (Parameters.TryGetValue("ProcessId", out var raw) &&
                int.TryParse(raw, out var pid))
            {
                return pid;
            }
            return null;
        }
        set
        {
            if (value is null)
                Parameters.Remove("ProcessId");
            else
                Parameters["ProcessId"] = value.Value.ToString();
        }
    }

    /// <summary>
    /// Tên process, lưu/đọc từ Parameters["ProcessName"]
    /// </summary>
    public string? ProcessName
    {
        get => Parameters.TryGetValue("ProcessName", out var v) ? v : null;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                Parameters.Remove("ProcessName");
            else
                Parameters["ProcessName"] = value;
        }
    }

    #endregion

    #region Factory helpers cho từng loại command

    public static CommandRequest CreateCaptureScreen(string deviceId)
        => new()
        {
            DeviceId = deviceId,
            Type = CommandType.CaptureScreen
        };

    public static CommandRequest CreateListProcesses(string deviceId)
        => new()
        {
            DeviceId = deviceId,
            Type = CommandType.ListProcesses
        };

    public static CommandRequest CreateKillProcess(string deviceId, int pid)
        => new()
        {
            DeviceId = deviceId,
            Type = CommandType.KillProcess,
            ProcessId = pid
        };

    public static CommandRequest CreateStartProcess(string deviceId, string processName)
        => new()
        {
            DeviceId = deviceId,
            Type = CommandType.StartProcess,
            ProcessName = processName
        };

    public static CommandRequest CreateGetSystemInfo(string deviceId)
        => new()
        {
            DeviceId = deviceId,
            Type = CommandType.GetSystemInfo
        };

    #endregion
}

public enum CommandType
{
    // ===== Application Management (APPLICATION) =====
    /// <summary>
    /// List all running applications (XEM command)
    /// </summary>
    ListApplications,

    /// <summary>
    /// Kill an application by ID (KILL -> KILLID command)
    /// Params: ProcessId
    /// </summary>
    KillApplication,

    /// <summary>
    /// Start an application (START -> STARTID command)
    /// Params: ProcessName
    /// </summary>
    StartApplication,

    // ===== Process Management (PROCESS) =====
    /// <summary>
    /// List all processes (XEM command)
    /// </summary>
    ListProcesses,

    /// <summary>
    /// Kill a process by ID (KILL -> KILLID command)
    /// Params: ProcessId
    /// </summary>
    KillProcess,

    /// <summary>
    /// Start a process (START -> STARTID command)
    /// Params: ProcessName
    /// </summary>
    StartProcess,

    // ===== Monitoring & System Info =====
    /// <summary>
    /// Get system information (CPU, RAM, process count)
    /// </summary>
    GetSystemInfo,

    // ===== Keylogger (KEYLOG) =====
    /// <summary>
    /// Start keylogger (HOOK command)
    /// </summary>
    StartKeylogger,

    /// <summary>
    /// Stop keylogger (UNHOOK command)
    /// </summary>
    StopKeylogger,

    /// <summary>
    /// Get keylog data (PRINT command)
    /// </summary>
    GetKeylogData,

    // ===== Screenshot (TAKEPIC) =====
    /// <summary>
    /// Capture screen (TAKE command)
    /// </summary>
    CaptureScreen,

    // ===== System Control (SHUTDOWN) =====
    /// <summary>
    /// Shutdown the system
    /// </summary>
    Shutdown,

    /// <summary>
    /// Restart the system
    /// </summary>
    Restart,

    /// <summary>
    /// Put system to sleep
    /// </summary>
    Sleep,

    /// <summary>
    /// Lock the workstation
    /// </summary>
    Lock,

    // ===== Webcam (future implementation) =====
    /// <summary>
    /// Turn on webcam
    /// </summary>
    WebcamOn,

    /// <summary>
    /// Turn off webcam
    /// </summary>
    WebcamOff,

    /// <summary>
    /// Capture webcam image
    /// </summary>
    CaptureWebcam,

    // ===== Registry (REGISTRY) =====
    /// <summary>
    /// Read registry value (Get value command)
    /// Params: Link, ValueName
    /// </summary>
    ReadRegistry,

    /// <summary>
    /// Write registry value (Set value command)
    /// Params: Link, ValueName, Value, TypeValue
    /// </summary>
    WriteRegistry,

    /// <summary>
    /// Create registry key (Create key command)
    /// Params: Link
    /// </summary>
    CreateRegistryKey,

    /// <summary>
    /// Delete registry key (Delete key command)
    /// Params: Link
    /// </summary>
    DeleteRegistryKey,

    /// <summary>
    /// Delete registry value (Delete value command)
    /// Params: Link, ValueName
    /// </summary>
    DeleteRegistryValue,

    // ===== Control =====
    /// <summary>
    /// Disconnect/quit (QUIT command)
    /// </summary>
    Quit
}

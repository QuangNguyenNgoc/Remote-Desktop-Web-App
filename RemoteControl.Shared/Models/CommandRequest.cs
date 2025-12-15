namespace RemoteControl.Shared.Models;

/// <summary>
/// Represents a command request from Web (Server) to Agent (Client)
/// Based on socket protocol: APPLICATION, SHUTDOWN, REGISTRY, TAKEPIC, PROCESS, KEYLOG, QUIT
/// </summary>
public class CommandRequest
{
    /// <summary>
    /// Unique identifier for this command request
    /// </summary>
    public string CommandId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Target agent ID
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// Type of command to execute
    /// </summary>
    public CommandType Type { get; set; }

    /// <summary>
    /// Additional parameters for the command (flexible key-value pairs)
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; } = new();

    /// <summary>
    /// When the command was requested
    /// </summary>
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Command types based on socket protocol analysis
/// Protocol commands: APPLICATION, SHUTDOWN, REGISTRY, TAKEPIC, PROCESS, KEYLOG
/// Sub-commands: XEM, KILL, START, KILLID, STARTID, HOOK, UNHOOK, PRINT, TAKE
/// </summary>
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

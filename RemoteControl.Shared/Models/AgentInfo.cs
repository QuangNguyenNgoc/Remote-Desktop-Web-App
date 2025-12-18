namespace RemoteControl.Shared.Models;

/// <summary>
/// Represents information about a connected agent/client
/// </summary>
public class AgentInfo
{
    /// <summary>
    /// Unique identifier for the agent
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// SignalR connection ID (assigned by Hub on connect)
    /// Used to route commands to specific agent
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// Machine/computer name
    /// </summary>
    public string MachineName { get; set; } = string.Empty;

    /// <summary>
    /// IP Address of the agent
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Operating system version
    /// </summary>
    public string OsVersion { get; set; } = string.Empty;

    /// <summary>
    /// When the agent first connected
    /// </summary>
    public DateTime ConnectedAt { get; set; }

    /// <summary>
    /// Last time the agent was seen/communicated
    /// </summary>
    public DateTime LastSeen { get; set; }

    /// <summary>
    /// Current agent status
    /// </summary>
    public AgentStatus Status { get; set; }

    /// <summary>
    /// System information (CPU, RAM, etc.)
    /// </summary>
    public SystemInfo? SystemInfo { get; set; }
}

/// <summary>
/// System resource information
/// </summary>
public class SystemInfo
{
    /// <summary>
    /// CPU usage percentage (0-100)
    /// </summary>
    public double CpuUsage { get; set; }

    /// <summary>
    /// Memory usage percentage (0-100)
    /// </summary>
    public double MemoryUsage { get; set; }

    /// <summary>
    /// Total memory in megabytes
    /// </summary>
    public long TotalMemoryMB { get; set; }

    /// <summary>
    /// Number of running processes
    /// </summary>
    public int ProcessCount { get; set; }

    /// <summary>
    /// Agent app uptime (time since Agent application started)
    /// Formatted as string for easy display (e.g., "2h 15m 30s")
    /// </summary>
    public string AgentUptime { get; set; } = "--";
}

/// <summary>
/// Agent connection status
/// </summary>
public enum AgentStatus
{
    /// <summary>
    /// Agent is connected and available
    /// </summary>
    Online,

    /// <summary>
    /// Agent is disconnected
    /// </summary>
    Offline,

    /// <summary>
    /// Agent is busy executing a command
    /// </summary>
    Busy,

    /// <summary>
    /// Agent encountered an error
    /// </summary>
    Error
}

namespace RemoteControl.Shared.Constants;

/// <summary>
/// SignalR Hub event names - Single source of truth for both Web and Agent
/// Prevents typos and enables refactoring
/// </summary>
public static class HubEvents
{
    // ===== Agent → Hub Events =====
    
    /// <summary>
    /// Agent registers with hub on first connect
    /// Payload: AgentInfo
    /// </summary>
    public const string RegisterAgent = "RegisterAgent";

    /// <summary>
    /// Agent sends command execution result
    /// Payload: CommandResult
    /// </summary>
    public const string SendResult = "SendResult";

    /// <summary>
    /// Agent sends periodic system metrics update
    /// Payload: SystemInfo
    /// </summary>
    public const string UpdateSystemInfo = "UpdateSystemInfo";

    /// <summary>
    /// Agent sends keep-alive signal
    /// Payload: string agentId
    /// </summary>
    public const string Heartbeat = "Heartbeat";

    // ===== Hub → Agent Events =====

    /// <summary>
    /// Hub sends command for agent to execute
    /// Payload: CommandRequest
    /// </summary>
    public const string ExecuteCommand = "ExecuteCommand";

    /// <summary>
    /// Hub forces agent to disconnect
    /// Payload: string reason
    /// </summary>
    public const string ForceDisconnect = "ForceDisconnect";

    // ===== Hub → Dashboard Events =====

    /// <summary>
    /// Broadcast when new agent connects
    /// Payload: AgentInfo
    /// </summary>
    public const string AgentConnected = "AgentConnected";

    /// <summary>
    /// Broadcast when agent disconnects
    /// Payload: string agentId
    /// </summary>
    public const string AgentDisconnected = "AgentDisconnected";

    /// <summary>
    /// Broadcast command execution result
    /// Payload: CommandResult
    /// </summary>
    public const string CommandCompleted = "CommandCompleted";

    /// <summary>
    /// Broadcast agent system info update
    /// Payload: (string agentId, SystemInfo systemInfo)
    /// </summary>
    public const string SystemInfoUpdated = "SystemInfoUpdated";

    // ===== Dashboard → Hub Methods =====

    /// <summary>
    /// Dashboard requests to send command to agent
    /// Payload: CommandRequest
    /// </summary>
    public const string SendCommand = "SendCommand";

    /// <summary>
    /// Dashboard requests current agent list
    /// Returns: List<AgentInfo>
    /// </summary>
    public const string GetAllAgents = "GetAllAgents";

    /// <summary>
    /// Dashboard requests to kick an agent
    /// Payload: (string agentId, string reason)
    /// </summary>
    public const string KickAgent = "KickAgent";
}

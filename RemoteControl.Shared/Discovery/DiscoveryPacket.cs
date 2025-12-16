namespace RemoteControl.Shared.Discovery;

/// <summary>
/// UDP Discovery packet for auto-discovery of servers on LAN.
/// Server broadcasts this packet, Agent listens.
/// </summary>
public class DiscoveryPacket
{
    /// <summary>
    /// UDP port used for discovery broadcast
    /// </summary>
    public const int DefaultPort = 5049;
    
    /// <summary>
    /// Packet identifier to filter valid discovery packets
    /// </summary>
    public const string PacketIdentifier = "REMOTECONTROL_DISCOVERY_V1";
    
    /// <summary>
    /// Broadcast interval in milliseconds
    /// </summary>
    public const int BroadcastIntervalMs = 3000;
    
    /// <summary>
    /// How long agent waits for discovery before fallback (ms)
    /// </summary>
    public const int DiscoveryTimeoutMs = 5000;

    /// <summary>
    /// Identifier to validate this is a RemoteControl discovery packet
    /// </summary>
    public string Identifier { get; set; } = PacketIdentifier;
    
    /// <summary>
    /// Full SignalR Hub URL (e.g., "http://192.168.1.100:5048/remotehub")
    /// </summary>
    public string HubUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Server machine name for display
    /// </summary>
    public string ServerName { get; set; } = string.Empty;
    
    /// <summary>
    /// When the packet was created
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using RemoteControl.Shared.Discovery;

namespace RemoteControl.Agent.Services;

/// <summary>
/// Listens for UDP broadcast packets from server to auto-discover Hub URL.
/// </summary>
public class DiscoveryListener
{
    /// <summary>
    /// Listen for server discovery broadcast and return the Hub URL.
    /// </summary>
    /// <param name="timeoutMs">How long to wait for a server (default 5000ms)</param>
    /// <returns>Hub URL if found, null if timeout</returns>
    public async Task<string?> DiscoverServerAsync(int timeoutMs = DiscoveryPacket.DiscoveryTimeoutMs)
    {
        Console.WriteLine($"[DiscoveryListener] Listening for server broadcast on UDP port {DiscoveryPacket.DefaultPort}...");
        
        using var udpClient = new UdpClient(DiscoveryPacket.DefaultPort);
        udpClient.EnableBroadcast = true;

        try
        {
            using var cts = new CancellationTokenSource(timeoutMs);
            
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    var result = await udpClient.ReceiveAsync(cts.Token);
                    var json = Encoding.UTF8.GetString(result.Buffer);
                    
                    var packet = JsonSerializer.Deserialize<DiscoveryPacket>(json);
                    
                    if (packet != null && packet.Identifier == DiscoveryPacket.PacketIdentifier)
                    {
                        Console.WriteLine($"[DiscoveryListener] Found server: {packet.ServerName} at {packet.HubUrl}");
                        return packet.HubUrl;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (JsonException)
                {
                    // Invalid packet, continue listening
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Timeout
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"[DiscoveryListener] Socket error: {ex.Message}");
        }

        Console.WriteLine("[DiscoveryListener] No server found within timeout.");
        return null;
    }

    /// <summary>
    /// Discover all servers broadcasting on the network.
    /// </summary>
    /// <param name="listenDurationMs">How long to listen for servers</param>
    /// <returns>List of discovered servers</returns>
    public async Task<List<DiscoveryPacket>> DiscoverAllServersAsync(int listenDurationMs = 5000)
    {
        var servers = new Dictionary<string, DiscoveryPacket>();
        
        Console.WriteLine($"[DiscoveryListener] Scanning for all servers for {listenDurationMs}ms...");
        
        using var udpClient = new UdpClient(DiscoveryPacket.DefaultPort);
        udpClient.EnableBroadcast = true;

        try
        {
            using var cts = new CancellationTokenSource(listenDurationMs);
            
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    var result = await udpClient.ReceiveAsync(cts.Token);
                    var json = Encoding.UTF8.GetString(result.Buffer);
                    
                    var packet = JsonSerializer.Deserialize<DiscoveryPacket>(json);
                    
                    if (packet != null && packet.Identifier == DiscoveryPacket.PacketIdentifier)
                    {
                        // Use HubUrl as key to avoid duplicates
                        servers[packet.HubUrl] = packet;
                        Console.WriteLine($"[DiscoveryListener] Found: {packet.ServerName} at {packet.HubUrl}");
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (JsonException)
                {
                    // Invalid packet, continue
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected timeout
        }

        Console.WriteLine($"[DiscoveryListener] Found {servers.Count} server(s).");
        return servers.Values.ToList();
    }
}

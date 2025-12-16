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
}

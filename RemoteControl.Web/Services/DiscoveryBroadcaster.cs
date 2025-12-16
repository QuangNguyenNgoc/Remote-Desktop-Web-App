using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using RemoteControl.Shared.Discovery;

namespace RemoteControl.Web.Services;

/// <summary>
/// Background service that broadcasts server discovery packets via UDP.
/// Agents on the LAN listen for these packets to auto-discover the server.
/// </summary>
public class DiscoveryBroadcaster : BackgroundService
{
    private readonly ILogger<DiscoveryBroadcaster> _logger;
    private readonly IConfiguration _configuration;
    private UdpClient? _udpClient;

    public DiscoveryBroadcaster(ILogger<DiscoveryBroadcaster> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[DiscoveryBroadcaster] Starting UDP broadcast service on port {Port}",
            DiscoveryPacket.DefaultPort);

        try
        {
            _udpClient = new UdpClient();
            _udpClient.EnableBroadcast = true;

            var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, DiscoveryPacket.DefaultPort);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var packet = CreateDiscoveryPacket();
                    var json = JsonSerializer.Serialize(packet);
                    var bytes = Encoding.UTF8.GetBytes(json);

                    await _udpClient.SendAsync(bytes, bytes.Length, broadcastEndpoint);
                    _logger.LogDebug("[DiscoveryBroadcaster] Broadcast sent: {HubUrl}", packet.HubUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("[DiscoveryBroadcaster] Broadcast failed: {Message}", ex.Message);
                }

                await Task.Delay(DiscoveryPacket.BroadcastIntervalMs, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DiscoveryBroadcaster] Fatal error");
        }
        finally
        {
            _udpClient?.Dispose();
            _logger.LogInformation("[DiscoveryBroadcaster] Stopped");
        }
    }

    private DiscoveryPacket CreateDiscoveryPacket()
    {
        var serverIp = GetLocalIPAddress();
        var port = GetServerPort();
        var hubUrl = $"http://{serverIp}:{port}/remotehub";

        return new DiscoveryPacket
        {
            Identifier = DiscoveryPacket.PacketIdentifier,
            HubUrl = hubUrl,
            ServerName = Environment.MachineName,
            Timestamp = DateTime.UtcNow
        };
    }

    private string GetLocalIPAddress()
    {
        try
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                var props = ni.GetIPProperties();
                foreach (var addr in props.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(addr.Address))
                    {
                        return addr.Address.ToString();
                    }
                }
            }
        }
        catch
        {
            // Fallback to DNS method
        }

        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        return "127.0.0.1";
    }

    private int GetServerPort()
    {
        var urls = _configuration["ASPNETCORE_URLS"] ?? _configuration["urls"];
        if (!string.IsNullOrEmpty(urls))
        {
            var uri = new Uri(urls.Split(';').First());
            return uri.Port;
        }
        return 5048;
    }
}

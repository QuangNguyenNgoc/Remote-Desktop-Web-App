using Microsoft.AspNetCore.SignalR.Client;
using RemoteControl.Agent.Handlers;
using RemoteControl.Shared.Models;
using RemoteControl.Shared.Constants;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RemoteControl.Agent.Services;

public class SignalRClientService
{
    private HubConnection? _hubConnection;
    private readonly CommandHandler _commandHandler;
    private readonly SystemInfoService _systemInfoService;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
    private readonly DiscoveryListener _discoveryListener;

    private readonly System.Timers.Timer _heartbeatTimer;
    private readonly System.Timers.Timer _systemInfoTimer;

    private bool _isConnected;
    private readonly string _agentId;
    private string? _manualHubUrl;  // URL set manually via dialog
    private int _sendingSystemInfo;

    public event Action<string>? OnStatusChanged;
    public event Action<string>? OnConnectionStateChanged;

    /// <summary>
    /// Set Hub URL manually (from connection dialog)
    /// </summary>
    public void SetHubUrl(string hubUrl)
    {
        _manualHubUrl = hubUrl;
    }

    public SignalRClientService(
        CommandHandler commandHandler,
        SystemInfoService systemInfoService,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _commandHandler = commandHandler;
        _systemInfoService = systemInfoService;
        _configuration = configuration;
        _discoveryListener = new DiscoveryListener();
        _agentId = Environment.MachineName;

        _heartbeatTimer = new System.Timers.Timer(10000); // 10s
        _heartbeatTimer.Elapsed += async (s, e) => await SendHeartbeat();

        _systemInfoTimer = new System.Timers.Timer(10000); // 10s
        _systemInfoTimer.Elapsed += async (s, e) => await SendSystemInfoAsync();

        // Wire webcam streaming callbacks
        _commandHandler.SetWebcamCallbacks(StartWebcam, StopWebcam);
    }

    public async Task ConnectAsync()
    {
        try
        {
            // Step 1: Get Hub URL (auto-discover or from config)
            var hubUrl = await GetHubUrlAsync();
            if (string.IsNullOrEmpty(hubUrl))
            {
                UpdateStatus("No server found. Check network or config.");
                OnConnectionStateChanged?.Invoke("Disconnected");
                return;
            }

            UpdateStatus($"Found server: {hubUrl}");

            // Step 2: Build HubConnection
          var passkey = _configuration["Passkey:Value"] ?? "";

_hubConnection = new HubConnectionBuilder()
    .WithUrl(hubUrl, options =>
    {
        if (!string.IsNullOrWhiteSpace(passkey))
        {
            options.Headers["X-Passkey"] = passkey;
        }
    })
    .WithAutomaticReconnect()
    .Build();


            _hubConnection.Reconnecting += OnReconnecting;
            _hubConnection.Reconnected += OnReconnected;
            _hubConnection.Closed += OnClosed;

            RegisterHandlers();

            // Step 3: Connect
            UpdateStatus("Connecting to Hub...");
            await _hubConnection.StartAsync();
            _isConnected = true;

            UpdateStatus("Connected to Hub");
            OnConnectionStateChanged?.Invoke("Connected");

            await RegisterAgentAsync();

            _heartbeatTimer.Start();
            _systemInfoTimer.Start();
        }
        catch (Exception ex)
        {
            UpdateStatus($"Connection failed: {ex.Message}");
            OnConnectionStateChanged?.Invoke("Disconnected");
            _isConnected = false;
        }
    }

    /// <summary>
    /// Try to get Hub URL: first manual, then config, then auto-discover
    /// </summary>
    private async Task<string?> GetHubUrlAsync()
    {
        // 1. Use manual URL if set (from connection dialog)
        if (!string.IsNullOrEmpty(_manualHubUrl))
        {
            UpdateStatus($"Using manual URL: {_manualHubUrl}");
            return _manualHubUrl;
        }

        // 2. Check if config exists and is not localhost
        var configUrl = _configuration["SignalR:HubUrl"];
        
        if (!string.IsNullOrEmpty(configUrl) && !configUrl.Contains("localhost"))
        {
            UpdateStatus($"Using configured URL: {configUrl}");
            return configUrl;
        }

        // 3. Try auto-discovery
        UpdateStatus("Auto-discovering server on LAN...");
        var discoveredUrl = await _discoveryListener.DiscoverServerAsync();
        
        if (!string.IsNullOrEmpty(discoveredUrl))
        {
            return discoveredUrl;
        }

        // 4. Fallback to config (even if localhost)
        if (!string.IsNullOrEmpty(configUrl))
        {
            UpdateStatus($"Fallback to config: {configUrl}");
            return configUrl;
        }

        // 5. Default localhost
        return "http://localhost:5048/remotehub";
    }

    private void RegisterHandlers()
    {
        if (_hubConnection == null) return;

        _hubConnection.On<CommandRequest>(HubEvents.ExecuteCommand, async (request) =>
        {
            UpdateStatus($"Received command: {request.Type}");
            var result = _commandHandler.HandleCommand(request);
            await SendResultAsync(result);
        });
    }

    private async Task RegisterAgentAsync()
    {
        if (_hubConnection == null) return;

        var info = new AgentInfo
        {
            AgentId = _agentId,
            MachineName = Environment.MachineName,
            IpAddress = GetLocalIPAddress(),
            OsVersion = Environment.OSVersion.ToString(),
            Status = AgentStatus.Online,
            ConnectedAt = DateTime.UtcNow,
            LastSeen = DateTime.UtcNow,
            SystemInfo = _systemInfoService.GetSystemInfo()
        };

        try
        {
            await _hubConnection.InvokeAsync(HubEvents.RegisterAgent, info);
            UpdateStatus("Agent Registered successfully");
        }
        catch (Exception ex)
        {
            UpdateStatus($"Registration failed: {ex.Message}");
        }
    }

    private async Task SendHeartbeat()
    {
        if (!_isConnected || _hubConnection == null) return;

        try
        {
            await _hubConnection.InvokeAsync("SendHeartbeat", _agentId);
        }
        catch
        {
            // Silent fail for heartbeat
        }
    }

    private async Task SendSystemInfoAsync()
    {
        if (!_isConnected || _hubConnection == null) return;

        if (Interlocked.Exchange(ref _sendingSystemInfo, 1) == 1)
            return;

        try
        {
            var info = _systemInfoService.GetSystemInfo();
            await _hubConnection.InvokeAsync(HubEvents.UpdateSystemInfo, _agentId, info);
        }
        catch
        {
            // Silent fail
        }
        finally
        {
            Interlocked.Exchange(ref _sendingSystemInfo, 0);
        }
    }

    private async Task SendResultAsync(CommandResult result)
    {
        if (!_isConnected || _hubConnection == null) return;
        try
        {
            await _hubConnection.InvokeAsync(HubEvents.SendResult, result);
            UpdateStatus($"Result sent for {result.CommandId}");
        }
        catch (Exception ex)
        {
            UpdateStatus($"Failed to send result: {ex.Message}");
        }
    }

    // ====== Webcam Streaming ======
    private WebCamService? _webCamService;
    
    public void SetWebCamService(WebCamService webCamService)
    {
        _webCamService = webCamService;
    }

    public void StartWebcam()
    {
        if (_webCamService == null) return;
        _webCamService.Start(async (frameBytes) => await SendWebcamFrame(frameBytes));
        Console.WriteLine("[SignalR] Webcam streaming started");
    }

    public void StopWebcam()
    {
        _webCamService?.Stop();
        Console.WriteLine("[SignalR] Webcam streaming stopped");
    }

    private async Task SendWebcamFrame(byte[] frameBytes)
    {
        if (!_isConnected || _hubConnection == null) return;
        try
        {
            var base64 = Convert.ToBase64String(frameBytes);
            await _hubConnection.InvokeAsync("SendWebcamFrame", _agentId, base64);
        }
        catch { /* Ignore frame drop errors */ }
    }

    private Task OnReconnecting(Exception? arg)
    {
        _isConnected = false;
        UpdateStatus("Reconnecting...");
        OnConnectionStateChanged?.Invoke("Reconnecting");
        return Task.CompletedTask;
    }

    private Task OnReconnected(string? arg)
    {
        _isConnected = true;
        UpdateStatus("Reconnected");
        OnConnectionStateChanged?.Invoke("Connected");
        _ = RegisterAgentAsync();
        _heartbeatTimer.Start();
        _systemInfoTimer.Start();
        return Task.CompletedTask;
    }

    private Task OnClosed(Exception? arg)
    {
        _isConnected = false;
        UpdateStatus($"Connection Closed: {arg?.Message}");
        OnConnectionStateChanged?.Invoke("Disconnected");
        _heartbeatTimer.Stop();
        _systemInfoTimer.Stop();
        return Task.CompletedTask;
    }

    private void UpdateStatus(string message)
    {
        OnStatusChanged?.Invoke(message);
    }

    private string GetLocalIPAddress()
    {
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
}

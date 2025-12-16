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
    private string? _manualHubUrl;
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

        _heartbeatTimer = new System.Timers.Timer(10000);
        _heartbeatTimer.Elapsed += async (s, e) => await SendHeartbeat();

        _systemInfoTimer = new System.Timers.Timer(10000);
        _systemInfoTimer.Elapsed += async (s, e) => await SendSystemInfoAsync();
    }

    public async Task ConnectAsync()
    {
        try
        {
            var hubUrl = await GetHubUrlAsync();
            if (string.IsNullOrEmpty(hubUrl))
            {
                UpdateStatus("No server found. Check network or config.");
                OnConnectionStateChanged?.Invoke("Disconnected");
                return;
            }

            UpdateStatus($"Found server: {hubUrl}");

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.Reconnecting += OnReconnecting;
            _hubConnection.Reconnected += OnReconnected;
            _hubConnection.Closed += OnClosed;

            RegisterHandlers();

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

    private async Task<string?> GetHubUrlAsync()
    {
        if (!string.IsNullOrEmpty(_manualHubUrl))
        {
            UpdateStatus($"Using manual URL: {_manualHubUrl}");
            return _manualHubUrl;
        }

        var configUrl = _configuration["SignalR:HubUrl"];
        
        if (!string.IsNullOrEmpty(configUrl) && !configUrl.Contains("localhost"))
        {
            UpdateStatus($"Using configured URL: {configUrl}");
            return configUrl;
        }

        UpdateStatus("Auto-discovering server on LAN...");
        var discoveredUrl = await _discoveryListener.DiscoverServerAsync();
        
        if (!string.IsNullOrEmpty(discoveredUrl))
        {
            return discoveredUrl;
        }

        if (!string.IsNullOrEmpty(configUrl))
        {
            UpdateStatus($"Fallback to config: {configUrl}");
            return configUrl;
        }

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
        catch { }
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
        catch { }
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

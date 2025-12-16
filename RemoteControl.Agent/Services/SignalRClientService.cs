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
    private readonly HubConnection _hubConnection;
    private readonly CommandHandler _commandHandler;
    private readonly SystemInfoService _systemInfoService;

    private readonly System.Timers.Timer _heartbeatTimer;
    private readonly System.Timers.Timer _systemInfoTimer;

    private bool _isConnected;
    private readonly string _agentId;

    private int _sendingSystemInfo; // chống overlap timer

    public event Action<string>? OnStatusChanged;
    public event Action<string>? OnConnectionStateChanged;

    public SignalRClientService(
        CommandHandler commandHandler,
        SystemInfoService systemInfoService,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _commandHandler = commandHandler;
        _systemInfoService = systemInfoService;

        _agentId = Environment.MachineName; // Simple Agent ID for now

        string hubUrl = configuration["SignalR:HubUrl"] ?? "http://localhost:5048/remotehub";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.Reconnecting += OnReconnecting;
        _hubConnection.Reconnected += OnReconnected;
        _hubConnection.Closed += OnClosed;

        _heartbeatTimer = new System.Timers.Timer(10000); // 10s
        _heartbeatTimer.Elapsed += async (s, e) => await SendHeartbeat();

        _systemInfoTimer = new System.Timers.Timer(10000); // 10s
        _systemInfoTimer.Elapsed += async (s, e) => await SendSystemInfoAsync();

        RegisterHandlers();
    }

    public async Task ConnectAsync()
    {
        try
        {
            UpdateStatus("Connecting to Hub...");
            await _hubConnection.StartAsync();
            _isConnected = true;

            UpdateStatus("Connected to Hub");
            OnConnectionStateChanged?.Invoke("Connected");

            await RegisterAgentAsync();

            _heartbeatTimer.Start();
            _systemInfoTimer.Start(); // (3) gửi system info định kỳ
        }
        catch (Exception ex)
        {
            UpdateStatus($"Connection failed: {ex.Message}");
            OnConnectionStateChanged?.Invoke("Disconnected");
            _isConnected = false;
        }
    }

    private void RegisterHandlers()
    {
        _hubConnection.On<CommandRequest>(HubEvents.ExecuteCommand, async (request) =>
        {
            UpdateStatus($"Received command: {request.Type}");

            // Execute locally
            var result = _commandHandler.HandleCommand(request);

            // Send result back
            await SendResultAsync(result);
        });
    }

    private async Task RegisterAgentAsync()
    {
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
        if (!_isConnected) return;

        try
        {
            // hiện hub bạn chưa có method này -> fail silent như bạn đang làm
            await _hubConnection.InvokeAsync("SendHeartbeat", _agentId);
        }
        catch
        {
            // Silent fail
        }
    }

    // (3) Timer tick: gửi SystemInfo lên hub
    private async Task SendSystemInfoAsync()
    {
        if (!_isConnected) return;

        if (Interlocked.Exchange(ref _sendingSystemInfo, 1) == 1)
            return;

        try
        {
            var info = _systemInfoService.GetSystemInfo();
            await _hubConnection.InvokeAsync(HubEvents.UpdateSystemInfo, _agentId, info);
        }
        catch
        {
            // Silent fail (tránh spam log)
        }
        finally
        {
            Interlocked.Exchange(ref _sendingSystemInfo, 0);
        }
    }

    private async Task SendResultAsync(CommandResult result)
    {
        if (!_isConnected) return;
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

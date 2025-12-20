// AgentWorker: Background service worker cho headless/service mode
// Chạy SignalR client và các services mà không cần UI
// Sử dụng Microsoft.Extensions.Hosting cho Windows Service support

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using RemoteControl.Agent.Handlers;
using RemoteControl.Agent.Services;

namespace RemoteControl.Agent.Workers;

/// <summary>
/// Background worker chạy Agent như Windows Service hoặc headless mode
/// </summary>
public class AgentWorker : BackgroundService
{
    // ====== Services ======
    private readonly IConfiguration _configuration;
    private readonly CommandHandler _commandHandler;
    private readonly SystemInfoService _systemInfoService;
    private readonly WebCamService _webCamService;
    private SignalRClientService? _signalRService;

    // ====== State ======
    private bool _isConnected = false;
    private readonly int _reconnectDelayMs = 5000;

    public AgentWorker(IConfiguration configuration)
    {
        _configuration = configuration;
        _commandHandler = new CommandHandler();
        _systemInfoService = new SystemInfoService();
        _webCamService = new WebCamService();

        Console.WriteLine("[AgentWorker] Initialized");
    }

    // ====== Main Execution Loop ======
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("[AgentWorker] Starting background service...");

        // Khởi tạo SignalR client
        _signalRService = new SignalRClientService(_commandHandler, _systemInfoService, _configuration);
        _signalRService.SetWebCamService(_webCamService);

        // Event handlers
        _signalRService.OnStatusChanged += (msg) => Console.WriteLine($"[AgentWorker] Status: {msg}");
        _signalRService.OnConnectionStateChanged += (state) => 
        {
            _isConnected = state == "Connected";
            Console.WriteLine($"[AgentWorker] Connection state: {state}");
        };

        // Main loop với auto-reconnect
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!_isConnected)
                {
                    Console.WriteLine("[AgentWorker] Connecting to server...");
                    await _signalRService.ConnectAsync();
                }

                // Wait và check connection status
                await Task.Delay(_reconnectDelayMs, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Service is stopping
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AgentWorker] Error: {ex.Message}");
                await Task.Delay(_reconnectDelayMs, stoppingToken);
            }
        }

        Console.WriteLine("[AgentWorker] Stopping background service...");
    }

    // ====== Cleanup on Stop ======
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("[AgentWorker] Stopping...");
        
        try
        {
            // Cleanup webcam
            _webCamService?.Stop();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AgentWorker] Cleanup error: {ex.Message}");
        }

        await base.StopAsync(cancellationToken);
    }
}

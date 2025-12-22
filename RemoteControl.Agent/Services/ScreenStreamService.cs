// ScreenStreamService: Stream màn hình real-time qua SignalR
// Capture loop với FPS điều chỉnh được

using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;
using RemoteControl.Shared.Constants;

namespace RemoteControl.Agent.Services;

public class ScreenStreamService
{
    private readonly ScreenshotService _screenshotService;
    private HubConnection? _hubConnection;
    
    private CancellationTokenSource? _streamCts;
    private bool _isStreaming = false;
    private int _currentFps = 10;
    private int _currentQuality = 70;
    
    // Stats
    public int ActualFps { get; private set; } = 0;
    public long LastFrameBytes { get; private set; } = 0;
    public bool IsStreaming => _isStreaming;

    public ScreenStreamService(ScreenshotService screenshotService)
    {
        _screenshotService = screenshotService;
    }

    public void SetHubConnection(HubConnection hubConnection)
    {
        _hubConnection = hubConnection;
    }

    /// <summary>
    /// Start streaming screen at specified FPS and quality
    /// </summary>
    public async Task StartStreamingAsync(int fps = 10, int quality = 70)
    {
        if (_isStreaming)
        {
            Console.WriteLine("[ScreenStream] Already streaming");
            return;
        }
        if (_hubConnection == null || _hubConnection.State != HubConnectionState.Connected)
        {
            Console.WriteLine("[ScreenStream] Hub not connected");
            return;
        }

        _currentFps = Math.Clamp(fps, 1, 30);
        _currentQuality = Math.Clamp(quality, 30, 100);
        _isStreaming = true;
        _streamCts = new CancellationTokenSource();

        Console.WriteLine($"[ScreenStream] Starting stream at {_currentFps} FPS, quality {_currentQuality}%");

        // Start capture loop in background
        _ = Task.Run(() => CaptureLoopAsync(_streamCts.Token));
    }

    /// <summary>
    /// Stop streaming
    /// </summary>
    public void StopStreaming()
    {
        if (!_isStreaming) return;

        _isStreaming = false;
        _streamCts?.Cancel();
        _streamCts?.Dispose();
        _streamCts = null;
        ActualFps = 0;

        Console.WriteLine("[ScreenStream] Streaming stopped");
    }

    /// <summary>
    /// Main capture loop - captures and sends frames at target FPS
    /// </summary>
    private async Task CaptureLoopAsync(CancellationToken ct)
    {
        var frameInterval = TimeSpan.FromMilliseconds(1000.0 / _currentFps);
        var stopwatch = new Stopwatch();
        var fpsStopwatch = Stopwatch.StartNew();
        int frameCount = 0;

        try
        {
            while (!ct.IsCancellationRequested && _isStreaming)
            {
                stopwatch.Restart();

                try
                {
                    // Capture screen
                    var screenshot = _screenshotService.CaptureScreen(_currentQuality);
                    
                    if (!string.IsNullOrEmpty(screenshot.ImageBase64))
                    {
                        LastFrameBytes = screenshot.ImageBase64.Length;
                        
                        // Send frame via SignalR
                        await _hubConnection!.InvokeAsync(
                            HubEvents.StreamFrame,
                            screenshot,
                            ct);
                        
                        frameCount++;
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"[ScreenStream] Frame error: {ex.Message}");
                }

                // Calculate actual FPS every second
                if (fpsStopwatch.ElapsedMilliseconds >= 1000)
                {
                    ActualFps = frameCount;
                    frameCount = 0;
                    fpsStopwatch.Restart();
                }

                // Wait for next frame (ensure target FPS)
                var elapsed = stopwatch.Elapsed;
                var delay = frameInterval - elapsed;
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, ct);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ScreenStream] Loop error: {ex.Message}");
        }
        finally
        {
            _isStreaming = false;
            ActualFps = 0;
            Console.WriteLine("[ScreenStream] Capture loop ended");
        }
    }

    /// <summary>
    /// Update stream settings while streaming
    /// </summary>
    public void UpdateSettings(int? fps = null, int? quality = null)
    {
        if (fps.HasValue) _currentFps = Math.Clamp(fps.Value, 1, 30);
        if (quality.HasValue) _currentQuality = Math.Clamp(quality.Value, 30, 100);
    }
}

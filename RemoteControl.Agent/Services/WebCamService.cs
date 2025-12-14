// WebCamService: Mở/Đóng Webcam và Stream dữ liệu
// Sử dụng thư viện AForge.Video.DirectShow

using System.Drawing;
using System.Drawing.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;

namespace RemoteControl.Agent.Services;

public class WebCamService
{
    private VideoCaptureDevice? _videoSource;
    private Action<byte[]>? _onFrameCallback;
    private readonly object _lock = new object();
    private bool _isRunning = false;

    // ====== Bắt đầu Stream Camera ======
    public void Start(Action<byte[]> onFrame)
    {
        if (_isRunning) return;

        try
        {
            // 1. Lấy danh sách thiết bị
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
            {
                Console.WriteLine("[WebCamService] No video devices found.");
                return;
            }

            // 2. Kết nối tới camera đầu tiên
            string deviceMoniker = videoDevices[0].MonikerString;
            _videoSource = new VideoCaptureDevice(deviceMoniker);
            _onFrameCallback = onFrame;

            // 3. Đăng ký sự kiện
            _videoSource.NewFrame += video_NewFrame;
            
            // 4. Bắt đầu
            _videoSource.Start();
            _isRunning = true;
            Console.WriteLine($"[WebCamService] Camera started: {videoDevices[0].Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WebCamService] Error starting camera: {ex.Message}");
            Stop();
        }
    }

    // ====== Dừng Stream ======
    public void Stop()
    {
        if (!_isRunning) return;

        try
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.WaitForStop();
                _videoSource.NewFrame -= video_NewFrame;
                _videoSource = null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WebCamService] Error stopping camera: {ex.Message}");
        }
        finally
        {
            _isRunning = false;
            _onFrameCallback = null;
            Console.WriteLine("[WebCamService] Camera stopped.");
        }
    }

    // ====== Callback xử lý frame ======
    private int _frameCount = 0;
    private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
    {
        if (_onFrameCallback == null) return;

        try
        {
            _frameCount++;
            if (_frameCount % 30 == 0) // Log mỗi 30 frames (khoảng 1 giây)
                Console.WriteLine($"[WebCamService] Frame #{_frameCount} received");

            using var ms = new MemoryStream();
            lock (_lock)
            {
                // Lật ảnh ngang (mirror) để hiển thị như gương
                var frame = (Bitmap)eventArgs.Frame.Clone();
                frame.RotateFlip(RotateFlipType.RotateNoneFlipX);
                frame.Save(ms, ImageFormat.Jpeg);
                frame.Dispose();
            }
            _onFrameCallback.Invoke(ms.ToArray());
        }
        catch (Exception ex) 
        { 
            Console.WriteLine($"[WebCamService] Frame error: {ex.Message}");
        }
    }
}

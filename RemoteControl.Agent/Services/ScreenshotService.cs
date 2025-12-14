// ScreenshotService: Chụp màn hình và convert sang Base64

using System.Drawing;
using System.Drawing.Imaging;
using RemoteControl.Shared.Models;

namespace RemoteControl.Agent.Services;

public class ScreenshotService
{
    // ====== Chụp toàn màn hình ======
    public ScreenshotResult CaptureScreen(int quality = 100)
    {
        try
        {
            // Lấy kích thước màn hình (VD: 1920x1080)
            var screenBounds = GetPrimaryScreenBounds();
            
            // Tạo Bitmap = vùng nhớ chứa pixels
            using var bitmap = new Bitmap(screenBounds.Width, screenBounds.Height);
            
            // Tạo Graphics = "cây bút" để vẽ lên Bitmap
            using var graphics = Graphics.FromImage(bitmap);
            
            // Copy pixels: màn hình → bitmap
            graphics.CopyFromScreen(
                screenBounds.X, screenBounds.Y, // nguồn (màn hình)
                0, 0,                           // đích (bitmap)
                screenBounds.Size,
                CopyPixelOperation.SourceCopy);
            
            // Convert → Base64 để gửi qua network
            string base64 = ConvertBitmapToBase64(bitmap, quality);
            
            return new ScreenshotResult
            {
                ImageBase64 = base64,
                Width = bitmap.Width,
                Height = bitmap.Height,
                CapturedAt = DateTime.UtcNow,
                Format = "jpeg"
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ScreenshotService] Error: {ex.Message}");
            return new ScreenshotResult
            {
                ImageBase64 = string.Empty,
                Width = 0,
                Height = 0,
                CapturedAt = DateTime.UtcNow,
                Format = "error"
            };
        }
    }
    
    // ====== Chụp 1 vùng cụ thể ======
    public ScreenshotResult CaptureRegion(int x, int y, int width, int height, int quality = 100)
    {
        try
        {
            using var bitmap = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(bitmap);
            
            graphics.CopyFromScreen(x, y, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
            
            string base64 = ConvertBitmapToBase64(bitmap, quality);
            
            return new ScreenshotResult
            {
                ImageBase64 = base64,
                Width = width,
                Height = height,
                CapturedAt = DateTime.UtcNow,
                Format = "jpeg"
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ScreenshotService] Error: {ex.Message}");
            return new ScreenshotResult
            {
                ImageBase64 = string.Empty,
                Width = 0,
                Height = 0,
                CapturedAt = DateTime.UtcNow,
                Format = "error"
            };
        }
    }
    
    // ====== Lấy kích thước màn hình chính ======
    private Rectangle GetPrimaryScreenBounds()
    {
        // DPI is handled globally in Program.cs
        // Windows API: lấy width/height màn hình
        int width = GetSystemMetrics(SM_CXSCREEN);
        int height = GetSystemMetrics(SM_CYSCREEN);
        return new Rectangle(0, 0, width, height);
    }
    
    // ====== Convert Bitmap → Base64 ======
    // Flow: Bitmap → stream → bytes → Base64 string
    private string ConvertBitmapToBase64(Bitmap bitmap, int quality)
    {
        using var stream = new MemoryStream();
        
        var jpegEncoder = GetEncoder(ImageFormat.Jpeg);
        
        if (jpegEncoder != null)
        {
            // Tạo tham số quality cho JPEG
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
            bitmap.Save(stream, jpegEncoder, encoderParams);
        }
        else
        {
            bitmap.Save(stream, ImageFormat.Png);
        }
        
        return Convert.ToBase64String(stream.ToArray());
    }
    
    // ====== Tìm encoder cho format ảnh ======
    private ImageCodecInfo? GetEncoder(ImageFormat format)
    {
        foreach (var codec in ImageCodecInfo.GetImageDecoders())
        {
            if (codec.FormatID == format.Guid)
                return codec;
        }
        return null;
    }
    
    // ====== Windows API ======
    private const int SM_CXSCREEN = 0;  // width
    private const int SM_CYSCREEN = 1;  // height
    
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
}

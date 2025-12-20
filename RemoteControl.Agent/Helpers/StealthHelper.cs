// StealthHelper: Windows API để ẩn/hiện console window và process
// Sử dụng P/Invoke để gọi user32.dll và kernel32.dll
// WARNING: Feature này có thể bị antivirus flag

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RemoteControl.Agent.Helpers;

/// <summary>
/// Helper class để ẩn/hiện process window và điều khiển visibility
/// </summary>
public static class StealthHelper
{
    // ====== Windows API Constants ======
    private const int SW_HIDE = 0;           // Ẩn window
    private const int SW_SHOW = 5;           // Hiện window
    private const int SW_MINIMIZE = 6;       // Minimize window
    private const int SW_RESTORE = 9;        // Restore window sau khi minimize

    // ====== P/Invoke Declarations ======
    
    /// <summary>
    /// Lấy handle của console window hiện tại
    /// </summary>
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    /// <summary>
    /// Điều khiển hiển thị của window
    /// </summary>
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    /// <summary>
    /// Lấy handle của window từ process ID
    /// </summary>
    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    /// <summary>
    /// Tìm window theo class name và title
    /// </summary>
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    /// <summary>
    /// Allocate console cho process
    /// </summary>
    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    /// <summary>
    /// Free console
    /// </summary>
    [DllImport("kernel32.dll")]
    private static extern bool FreeConsole();

    // ====== Current Process Window Handle (cached) ======
    private static IntPtr _cachedWindowHandle = IntPtr.Zero;

    /// <summary>
    /// Ẩn console window của process hiện tại
    /// </summary>
    /// <returns>true nếu thành công</returns>
    public static bool HideConsoleWindow()
    {
        try
        {
            var handle = GetConsoleWindow();
            if (handle != IntPtr.Zero)
            {
                _cachedWindowHandle = handle;
                return ShowWindow(handle, SW_HIDE);
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[StealthHelper] HideConsoleWindow error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Hiện lại console window đã ẩn
    /// </summary>
    /// <returns>true nếu thành công</returns>
    public static bool ShowConsoleWindow()
    {
        try
        {
            var handle = _cachedWindowHandle != IntPtr.Zero ? _cachedWindowHandle : GetConsoleWindow();
            if (handle != IntPtr.Zero)
            {
                return ShowWindow(handle, SW_SHOW);
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[StealthHelper] ShowConsoleWindow error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Ẩn window của một Form (WinForms)
    /// </summary>
    /// <param name="formHandle">Handle của form</param>
    /// <returns>true nếu thành công</returns>
    public static bool HideWindow(IntPtr formHandle)
    {
        try
        {
            if (formHandle != IntPtr.Zero)
            {
                return ShowWindow(formHandle, SW_HIDE);
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[StealthHelper] HideWindow error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Hiện window của một Form
    /// </summary>
    /// <param name="formHandle">Handle của form</param>
    /// <returns>true nếu thành công</returns>
    public static bool ShowFormWindow(IntPtr formHandle)
    {
        try
        {
            if (formHandle != IntPtr.Zero)
            {
                return ShowWindow(formHandle, SW_SHOW);
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[StealthHelper] ShowFormWindow error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Minimize window to taskbar
    /// </summary>
    /// <param name="formHandle">Handle của form</param>
    /// <returns>true nếu thành công</returns>
    public static bool MinimizeWindow(IntPtr formHandle)
    {
        try
        {
            if (formHandle != IntPtr.Zero)
            {
                return ShowWindow(formHandle, SW_MINIMIZE);
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[StealthHelper] MinimizeWindow error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Kiểm tra xem process có đang chạy với console hay không
    /// </summary>
    /// <returns>true nếu có console</returns>
    public static bool HasConsole()
    {
        return GetConsoleWindow() != IntPtr.Zero;
    }

    /// <summary>
    /// Detach khỏi console (cho GUI apps xuất phát từ console)
    /// </summary>
    /// <returns>true nếu thành công</returns>
    public static bool DetachConsole()
    {
        try
        {
            return FreeConsole();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Attach vào console mới (useful for debugging)
    /// </summary>
    /// <returns>true nếu thành công</returns>
    public static bool AttachNewConsole()
    {
        try
        {
            return AllocConsole();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Lấy tên của process hiện tại (để check trong Task Manager)
    /// </summary>
    /// <returns>Process name</returns>
    public static string GetCurrentProcessName()
    {
        try
        {
            return Process.GetCurrentProcess().ProcessName;
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// Kiểm tra xem process có đang chạy với quyền Administrator không
    /// </summary>
    /// <returns>true nếu có quyền admin</returns>
    public static bool IsRunningAsAdmin()
    {
        try
        {
            using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }
}

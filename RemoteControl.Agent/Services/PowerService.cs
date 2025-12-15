using System.Runtime.InteropServices;
using System.Diagnostics;

namespace RemoteControl.Agent.Services;

public class PowerService
{
    // Import user32.dll for LockWorkStation
    [DllImport("user32.dll")]
    public static extern bool LockWorkStation();

    // Import powrprof.dll for SetSuspendState (Sleep/Hibernate)
    [DllImport("PowrProf.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);

    public (bool Success, string Message) Shutdown()
    {
        try
        {
            Process.Start("shutdown", "/s /t 0");
            return (true, "Shutdown command sent");
        }
        catch (Exception ex)
        {
            return (false, $"Shutdown failed: {ex.Message}");
        }
    }

    public (bool Success, string Message) Restart()
    {
        try
        {
            Process.Start("shutdown", "/r /t 0");
            return (true, "Restart command sent");
        }
        catch (Exception ex)
        {
            return (false, $"Restart failed: {ex.Message}");
        }
    }

    public (bool Success, string Message) Sleep()
    {
        try
        {
            // hibernate=false (Sleep), forceCritical=true, disableWakeEvent=false
            bool result = SetSuspendState(false, true, false);
            return result ? (true, "System entering sleep mode") : (false, "Failed to invoke SetSuspendState");
        }
        catch (Exception ex)
        {
            return (false, $"Sleep failed: {ex.Message}");
        }
    }

    public (bool Success, string Message) Lock()
    {
        try
        {
            bool result = LockWorkStation();
            return result ? (true, "Workstation locked") : (false, "Failed to lock workstation used user32.dll");
        }
        catch (Exception ex)
        {
            return (false, $"Lock failed: {ex.Message}");
        }
    }
}

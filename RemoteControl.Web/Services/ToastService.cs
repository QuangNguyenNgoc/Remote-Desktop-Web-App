using System;
using System.Collections.Generic;
using System.Timers;

namespace RemoteControl.Web.Services;

/// <summary>
/// Toast notification types
/// </summary>
public enum ToastType
{
    Success,
    Error,
    Warning,
    Info
}

/// <summary>
/// Individual toast notification
/// </summary>
public class ToastMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ToastType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int DurationMs { get; set; } = 4000; // Auto-dismiss after 4 seconds
}

/// <summary>
/// Scoped service to manage toast notifications across the app
/// </summary>
public class ToastService : IDisposable
{
    private readonly List<ToastMessage> _toasts = new();
    private readonly object _lock = new();
    
    /// <summary>
    /// Event fired when toast list changes
    /// </summary>
    public event Action? OnChange;
    
    /// <summary>
    /// Get all active toasts
    /// </summary>
    public IReadOnlyList<ToastMessage> Toasts
    {
        get
        {
            lock (_lock)
            {
                return _toasts.ToList();
            }
        }
    }
    
    /// <summary>
    /// Show success toast (green)
    /// </summary>
    public void ShowSuccess(string message, string title = "Success")
    {
        Show(ToastType.Success, title, message);
    }
    
    /// <summary>
    /// Show error toast (red)
    /// </summary>
    public void ShowError(string message, string title = "Error")
    {
        Show(ToastType.Error, title, message, 6000); // Errors stay longer
    }
    
    /// <summary>
    /// Show warning toast (yellow)
    /// </summary>
    public void ShowWarning(string message, string title = "Warning")
    {
        Show(ToastType.Warning, title, message);
    }
    
    /// <summary>
    /// Show info toast (blue)
    /// </summary>
    public void ShowInfo(string message, string title = "Info")
    {
        Show(ToastType.Info, title, message);
    }
    
    private void Show(ToastType type, string title, string message, int durationMs = 4000)
    {
        var toast = new ToastMessage
        {
            Type = type,
            Title = title,
            Message = message,
            DurationMs = durationMs
        };
        
        lock (_lock)
        {
            _toasts.Add(toast);
            // Keep max 5 toasts
            while (_toasts.Count > 5)
            {
                _toasts.RemoveAt(0);
            }
        }
        
        OnChange?.Invoke();
        
        // Auto-remove after duration
        var timer = new Timer(durationMs);
        timer.Elapsed += (s, e) =>
        {
            Remove(toast.Id);
            timer.Dispose();
        };
        timer.AutoReset = false;
        timer.Start();
    }
    
    /// <summary>
    /// Manually remove a toast
    /// </summary>
    public void Remove(Guid id)
    {
        lock (_lock)
        {
            _toasts.RemoveAll(t => t.Id == id);
        }
        OnChange?.Invoke();
    }
    
    public void Dispose()
    {
        lock (_lock)
        {
            _toasts.Clear();
        }
    }
}

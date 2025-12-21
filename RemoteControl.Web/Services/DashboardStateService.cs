using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR.Client;
using RemoteControl.Shared.Models;
using RemoteControl.Shared.Constants;

namespace RemoteControl.Web.Services;

/// <summary>
/// Activity model for dashboard recent activities
/// </summary>
public class AgentActivityModel
{
    public string AgentId { get; set; } = "";
    public string FilterKey { get; set; } = ""; // Connect, Disconnect, Command
    public string EventType { get; set; } = "";
    public string BadgeTextColor { get; set; } = "";
    public string BadgeBgColor { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Details { get; set; } = "";
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string TimeText { get; set; } = "—"; // For UI display
}

/// <summary>
/// Scoped service that persists dashboard state across page navigation.
/// Single SignalR connection shared across components.
/// </summary>
public class DashboardStateService : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly object _lock = new();

    // ========== Agent State ==========
    public HashSet<string> KnownAgentIds { get; } = new(StringComparer.Ordinal);
    public HashSet<string> OnlineAgentIds { get; } = new(StringComparer.Ordinal);
    public int TotalAgents => KnownAgentIds.Count;
    public int OnlineAgents => OnlineAgentIds.Count;
    public int OfflineAgents => Math.Max(0, TotalAgents - OnlineAgents);

    // ========== Dashboard Data (PERSISTED) ==========
    public List<(DateTime tUtc, int online)> OnlineSeries { get; } = new();
    public Dictionary<string, int> CommandCounts { get; } = new(StringComparer.Ordinal)
    {
        ["Capture"] = 0,
        ["Process"] = 0,
        ["Keylog"] = 0,
        ["Registry"] = 0,
        ["Other"] = 0
    };
    public List<AgentActivityModel> RecentActivities { get; } = new();

    // ========== App-level State ==========
    public DateTime AppStartUtc { get; } = DateTime.UtcNow;
    public DateTime LastSyncUtc { get; private set; } = DateTime.UtcNow;
    public int CommandsThisSession { get; private set; } = 0;

    // ========== Events ==========
    public event Action? OnStateChanged;
    
    // Connection state
    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
    public bool IsInitialized { get; private set; } = false;

    // ========== Methods ==========
    
    /// <summary>
    /// Initialize SignalR connection (call once from first component)
    /// </summary>
    public async Task InitializeAsync(string hubUrl)
    {
        if (IsInitialized) return;
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        SetupEventHandlers();
        
        await _hubConnection.StartAsync();
        await FetchInitialAgentsAsync();
        
        IsInitialized = true;
        AddOnlinePoint();
        NotifyStateChanged();
    }

    private void SetupEventHandlers()
    {
        if (_hubConnection == null) return;

        _hubConnection.On<AgentInfo>(HubEvents.AgentConnected, agent =>
        {
            TrackAgentOnline(agent);
            PushActivity(new AgentActivityModel
            {
                AgentId = agent.AgentId,
                FilterKey = "Connect",
                EventType = "Connect",
                BadgeTextColor = "var(--status-online)",
                BadgeBgColor = "var(--status-badge-online-bg)",
                Summary = "Agent connected",
                Details = "Connected to Hub",
                TimestampUtc = DateTime.UtcNow
            });
            AddOnlinePoint();
            NotifyStateChanged();
        });

        _hubConnection.On<string>(HubEvents.AgentDisconnected, agentId =>
        {
            TrackAgentOffline(agentId);
            PushActivity(new AgentActivityModel
            {
                AgentId = agentId,
                FilterKey = "Disconnect",
                EventType = "Disconnect",
                BadgeTextColor = "var(--btn-stop-bg)",
                BadgeBgColor = "rgba(239, 68, 68, 0.12)",
                Summary = "Agent disconnected",
                Details = "Disconnected from Hub",
                TimestampUtc = DateTime.UtcNow
            });
            AddOnlinePoint();
            NotifyStateChanged();
        });

        _hubConnection.On<CommandResult>(HubEvents.CommandCompleted, result =>
        {
            CommandsThisSession++;
            
            var bucket = ClassifyCommand(result);
            if (CommandCounts.ContainsKey(bucket)) CommandCounts[bucket]++;
            else CommandCounts["Other"]++;

            PushActivity(new AgentActivityModel
            {
                AgentId = result.AgentId,
                FilterKey = "Command",
                EventType = "Command",
                BadgeTextColor = result.Success ? "var(--status-online)" : "var(--btn-stop-bg)",
                BadgeBgColor = result.Success ? "var(--status-badge-online-bg)" : "rgba(239, 68, 68, 0.12)",
                Summary = $"Command completed (Success={result.Success})",
                Details = $"CommandId: {result.CommandId}\nSuccess: {result.Success}\nMessage: {result.Message}",
                TimestampUtc = DateTime.UtcNow
            });
            NotifyStateChanged();
        });
    }

    private async Task FetchInitialAgentsAsync()
    {
        if (_hubConnection == null) return;
        
        try
        {
            var agents = await _hubConnection.InvokeAsync<List<AgentInfo>>(HubEvents.GetAllAgents);
            
            lock (_lock)
            {
                KnownAgentIds.Clear();
                OnlineAgentIds.Clear();
                
                foreach (var a in agents)
                {
                    if (string.IsNullOrWhiteSpace(a.AgentId)) continue;
                    KnownAgentIds.Add(a.AgentId);
                    if (a.Status == AgentStatus.Online) OnlineAgentIds.Add(a.AgentId);
                }
            }
            LastSyncUtc = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DashboardStateService] FetchInitialAgents error: {ex.Message}");
        }
    }

    public void TrackAgentOnline(AgentInfo agent)
    {
        if (string.IsNullOrWhiteSpace(agent.AgentId)) return;
        lock (_lock)
        {
            KnownAgentIds.Add(agent.AgentId);
            OnlineAgentIds.Add(agent.AgentId);
        }
        LastSyncUtc = DateTime.UtcNow;
    }

    public void TrackAgentOffline(string agentId)
    {
        if (string.IsNullOrWhiteSpace(agentId)) return;
        lock (_lock)
        {
            OnlineAgentIds.Remove(agentId);
        }
        LastSyncUtc = DateTime.UtcNow;
    }

    public void PushActivity(AgentActivityModel activity)
    {
        activity.TimeText = DateTime.Now.ToString("HH:mm:ss");
        lock (_lock)
        {
            RecentActivities.Insert(0, activity);
            // Keep max 100 activities
            while (RecentActivities.Count > 100)
            {
                RecentActivities.RemoveAt(RecentActivities.Count - 1);
            }
        }
    }

    public void AddOnlinePoint()
    {
        var now = DateTime.UtcNow;
        lock (_lock)
        {
            OnlineSeries.Add((now, OnlineAgents));
            
            // Prune old data (older than 24h)
            var cutoff = now.AddHours(-24);
            OnlineSeries.RemoveAll(p => p.tUtc < cutoff);
        }
    }

    private string ClassifyCommand(CommandResult result)
    {
        var type = result.Message ?? "";
        
        if (type.Contains("Screenshot", StringComparison.OrdinalIgnoreCase) || 
            type.Contains("Capture", StringComparison.OrdinalIgnoreCase) || 
            type.Contains("Webcam", StringComparison.OrdinalIgnoreCase))
            return "Capture";
        if (type.Contains("Process", StringComparison.OrdinalIgnoreCase) || 
            type.Contains("Kill", StringComparison.OrdinalIgnoreCase))
            return "Process";
        if (type.Contains("Keylog", StringComparison.OrdinalIgnoreCase))
            return "Keylog";
        if (type.Contains("Registry", StringComparison.OrdinalIgnoreCase))
            return "Registry";
        
        return "Other";
    }

    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }

    // ========== Chart Data Helpers ==========
    
    public (List<string> labels, List<int> values) GetOnlineSeriesData()
    {
        var labels = new List<string>();
        var values = new List<int>();
        
        lock (_lock)
        {
            foreach (var (tUtc, online) in OnlineSeries.TakeLast(24))
            {
                labels.Add(tUtc.ToLocalTime().ToString("HH:mm"));
                values.Add(online);
            }
        }
        
        return (labels, values);
    }

    public (List<string> labels, List<int> values) GetCommandCountsData()
    {
        var order = new[] { "Capture", "Process", "Keylog", "Registry", "Other" };
        var labels = order.ToList();
        var values = order.Select(k => CommandCounts.TryGetValue(k, out var v) ? v : 0).ToList();
        return (labels, values);
    }

    // ========== IAsyncDisposable ==========
    
    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}

// RemoteControlHub: SignalR Hub trung tâm để Web và Agent nói chuyện với nhau
// - RegisterAgent: agent đăng ký kết nối, lưu vào Dictionary
// - SendCommand: web gửi command tới đúng agent (dùng request.AgentId)
// - SendResult: agent trả kết quả command, broadcast cho dashboard
// - UpdateSystemInfo: agent gửi system info định kỳ, broadcast SystemInfoUpdated(agentId, systemInfo)

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RemoteControl.Shared.Models;
using RemoteControl.Shared.Constants; // HubEvents constants

namespace RemoteControl.Web.Hubs
{
    public class RemoteControlHub : Hub
    {
        private readonly ILogger<RemoteControlHub> _logger;

        private static readonly ConcurrentDictionary<string, AgentInfo> ConnectedAgents
            = new ConcurrentDictionary<string, AgentInfo>();

        public RemoteControlHub(ILogger<RemoteControlHub> logger)
        {
            _logger = logger;
        }

        public async Task RegisterAgent(AgentInfo agent)
        {
            var connectionId = Context.ConnectionId;

            try
            {
                if (agent == null || string.IsNullOrWhiteSpace(agent.AgentId))
                {
                    _logger.LogWarning("RegisterAgent: agent không hợp lệ từ {ConnectionId}", connectionId);
                    return;
                }

                agent.ConnectionId = connectionId;
                agent.Status = AgentStatus.Online;
                agent.LastSeen = DateTime.UtcNow;
                ConnectedAgents[agent.AgentId] = agent;

                _logger.LogInformation(
                    "RegisterAgent: Agent {AgentId} ({MachineName}) đăng ký với Connection {ConnectionId}",
                    agent.AgentId, agent.MachineName, connectionId);

                await Clients.All.SendAsync(HubEvents.AgentConnected, agent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "RegisterAgent failed for Connection {ConnectionId}, AgentId={AgentId}",
                    connectionId, agent?.AgentId ?? "null");
            }
        }

        public List<AgentInfo> GetAllAgents()
        {
            var agents = ConnectedAgents.Values.ToList();
            _logger.LogInformation("GetAllAgents: Trả về {Count} agents", agents.Count);
            return agents;
        }

        public async Task SendCommand(CommandRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.AgentId))
                {
                    _logger.LogWarning("SendCommand: request không hợp lệ (thiếu AgentId)");
                    return;
                }

                var targetAgentId = request.AgentId;

                if (!ConnectedAgents.TryGetValue(targetAgentId, out var agentInfo))
                {
                    _logger.LogWarning("SendCommand: Agent {AgentId} không kết nối", targetAgentId);

                    await Clients.Caller.SendAsync(
                        "CommandFailed",
                        request,
                        $"Agent {targetAgentId} đang offline");

                    return;
                }

                var agentConnectionId = agentInfo.ConnectionId;

                _logger.LogInformation(
                    "SendCommand: Command {CommandId} ({CommandType}) -> Agent {AgentId}",
                    request.CommandId,
                    request.Type,
                    targetAgentId);

                await Clients.Client(agentConnectionId).SendAsync(HubEvents.ExecuteCommand, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "SendCommand failed: CommandId={CommandId}, AgentId={AgentId}",
                    request?.CommandId ?? "null", request?.AgentId ?? "null");

                // Notify caller of failure
                await Clients.Caller.SendAsync(
                    "CommandFailed",
                    request,
                    $"Internal error: {ex.Message}");
            }
        }

        public async Task SendResult(CommandResult result)
        {
            try
            {
                if (result == null || string.IsNullOrWhiteSpace(result.AgentId))
                {
                    _logger.LogWarning("SendResult: result không hợp lệ");
                    return;
                }

                _logger.LogInformation(
                    "SendResult: Command {CommandId} from Agent {AgentId}, Success={Success}, Message={Message}",
                    result.CommandId,
                    result.AgentId,
                    result.Success,
                    result.Message ?? "(none)");

                await Clients.All.SendAsync(HubEvents.CommandCompleted, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "SendResult failed: CommandId={CommandId}, AgentId={AgentId}",
                    result?.CommandId ?? "null", result?.AgentId ?? "null");
            }
        }

        // =========================
        // (2) SystemInfo: Agent -> Hub -> Dashboard
        // broadcast: SystemInfoUpdated(agentId, systemInfo)
        // =========================
        public async Task UpdateSystemInfo(string agentId, SystemInfo systemInfo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(agentId) || systemInfo == null)
                {
                    _logger.LogWarning("UpdateSystemInfo: payload không hợp lệ, AgentId={AgentId}", agentId ?? "null");
                    return;
                }

                if (ConnectedAgents.TryGetValue(agentId, out var agent))
                {
                    agent.SystemInfo = systemInfo;
                    agent.LastSeen = DateTime.UtcNow;
                    agent.Status = AgentStatus.Online;
                    ConnectedAgents[agentId] = agent;
                }

                await Clients.All.SendAsync(HubEvents.SystemInfoUpdated, agentId, systemInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateSystemInfo failed for AgentId={AgentId}", agentId ?? "null");
            }
        }

        // =========================
        // (3) Webcam: Agent -> Hub -> Web client
        // broadcast: WebcamFrame(agentId, base64Jpeg)
        // =========================
        public async Task SendWebcamFrame(string agentId, string base64Jpeg)
        {
            if (string.IsNullOrWhiteSpace(agentId) || string.IsNullOrWhiteSpace(base64Jpeg))
            {
                return; // Silent drop - don't log every frame
            }

            // Broadcast to all clients watching this agent
            await Clients.All.SendAsync("WebcamFrame", agentId, base64Jpeg);
        }

        // =========================
        // (4) Screen Streaming: Dashboard <-> Agent
        // =========================

        /// <summary>
        /// Dashboard requests agent to start screen streaming
        /// </summary>
        public async Task StartScreenStream(string agentId, int fps = 10, int quality = 70)
        {
            if (string.IsNullOrWhiteSpace(agentId))
            {
                _logger.LogWarning("StartScreenStream: AgentId is null");
                return;
            }

            if (ConnectedAgents.TryGetValue(agentId, out var agent))
            {
                _logger.LogInformation("StartScreenStream requested for {AgentId} at {FPS} FPS", agentId, fps);
                await Clients.Client(agent.ConnectionId).SendAsync(HubEvents.StartScreenStream, fps, quality);
            }
        }

        /// <summary>
        /// Dashboard requests agent to stop screen streaming
        /// </summary>
        public async Task StopScreenStream(string agentId)
        {
            if (string.IsNullOrWhiteSpace(agentId)) return;

            if (ConnectedAgents.TryGetValue(agentId, out var agent))
            {
                _logger.LogInformation("StopScreenStream requested for {AgentId}", agentId);
                await Clients.Client(agent.ConnectionId).SendAsync(HubEvents.StopScreenStream);
            }
        }

        /// <summary>
        /// Agent sends a screen frame during streaming
        /// </summary>
        public async Task StreamFrame(ScreenshotResult frame)
        {
            // Get agentId from connection
            var agentId = ConnectedAgents.Values.FirstOrDefault(a => a.ConnectionId == Context.ConnectionId)?.AgentId;
            if (string.IsNullOrWhiteSpace(agentId)) return;

            // Broadcast frame to all clients (dashboard will filter by agentId)
            await Clients.All.SendAsync(HubEvents.ScreenFrameReceived, agentId, frame);
        }


        public override Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            foreach (var kvp in ConnectedAgents)
            {
                if (kvp.Value.ConnectionId == connectionId)
                {
                    ConnectedAgents.TryRemove(kvp.Key, out _);
                    _logger.LogInformation(
                        "OnDisconnected: xoá Agent {AgentId} (Conn {ConnectionId})",
                        kvp.Key, connectionId);

                    await Clients.All.SendAsync(HubEvents.AgentDisconnected, kvp.Key);
                }
            }

            if (exception != null)
            {
                _logger.LogWarning(exception,
                    "Client disconnected with error: {ConnectionId}", connectionId);
            }
            else
            {
                _logger.LogInformation("Client disconnected: {ConnectionId}", connectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}

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

        public List<AgentInfo> GetAllAgents()
        {
            var agents = ConnectedAgents.Values.ToList();
            _logger.LogInformation("GetAllAgents: Trả về {Count} agents", agents.Count);
            return agents;
        }

        public async Task SendCommand(CommandRequest request)
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
                "SendCommand: Command {CommandId} -> Agent {AgentId} (Conn {ConnectionId})",
                request.CommandId,
                targetAgentId,
                agentConnectionId);

            await Clients.Client(agentConnectionId).SendAsync(HubEvents.ExecuteCommand, request);
        }

        public async Task SendResult(CommandResult result)
        {
            if (result == null || string.IsNullOrWhiteSpace(result.AgentId))
            {
                _logger.LogWarning("SendResult: result không hợp lệ");
                return;
            }

            _logger.LogInformation(
                "SendResult: Command {CommandId} từ Agent {AgentId}, Success = {Success}",
                result.CommandId,
                result.AgentId,
                result.Success);

            await Clients.All.SendAsync(HubEvents.CommandCompleted, result);
        }

        // =========================
        // (2) SystemInfo: Agent -> Hub -> Dashboard
        // broadcast: SystemInfoUpdated(agentId, systemInfo)
        // =========================
        public async Task UpdateSystemInfo(string agentId, SystemInfo systemInfo)
        {
            if (string.IsNullOrWhiteSpace(agentId) || systemInfo == null)
            {
                _logger.LogWarning("UpdateSystemInfo: payload không hợp lệ");
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

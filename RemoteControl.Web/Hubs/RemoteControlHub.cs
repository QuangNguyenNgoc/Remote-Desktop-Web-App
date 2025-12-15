// RemoteControlHub: SignalR Hub trung tâm để Web và Agent nói chuyện với nhau
// - RegisterAgent: agent đăng ký kết nối, lưu vào Dictionary
// - SendCommand: web gửi command tới đúng agent (dùng request.AgentId)
// - SendResult: agent trả kết quả command, broadcast cho dashboard

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RemoteControl.Shared.Models;
using RemoteControl.Shared.Constants; // HubEvents constants

namespace RemoteControl.Web.Hubs
{
    public class RemoteControlHub : Hub
    {
        // Logger để ghi log cho từng action trong hub
        private readonly ILogger<RemoteControlHub> _logger;

        // Bảng lưu ánh xạ: AgentId -> AgentInfo (full info including ConnectionId)
        // ConcurrentDictionary dùng cho môi trường multi-thread (nhiều kết nối cùng lúc)
        private static readonly ConcurrentDictionary<string, AgentInfo> ConnectedAgents
            = new ConcurrentDictionary<string, AgentInfo>();

        public RemoteControlHub(ILogger<RemoteControlHub> logger)
        {
            _logger = logger;
        }

        // ==========================
        // 1) Agent đăng ký với server
        // ==========================
        // - Được gọi từ phía Agent khi connect xong
        // - Lưu AgentId kèm ConnectionId hiện tại vào dictionary
        // - Broadcast cho tất cả client biết có agent mới
        public async Task RegisterAgent(AgentInfo agent)
        {
            var connectionId = Context.ConnectionId; // id của kết nối hiện tại

            // Validate input
            if (agent == null || string.IsNullOrWhiteSpace(agent.AgentId))
            {
                _logger.LogWarning("RegisterAgent: agent không hợp lệ từ {ConnectionId}", connectionId);
                return;
            }

            // Lưu / cập nhật AgentInfo với ConnectionId
            agent.ConnectionId = connectionId;
            agent.Status = AgentStatus.Online;
            agent.LastSeen = DateTime.UtcNow;
            ConnectedAgents[agent.AgentId] = agent;

            _logger.LogInformation(
                "RegisterAgent: Agent {AgentId} ({MachineName}) đăng ký với Connection {ConnectionId}",
                agent.AgentId, agent.MachineName, connectionId);

            // Gửi sự kiện AgentConnected cho tất cả client (dashboard, log viewer, …)
            await Clients.All.SendAsync(HubEvents.AgentConnected, agent);
        }

        // ==========================
        // 2) Lấy danh sách tất cả agents (cho Dashboard)
        // ==========================
        public List<AgentInfo> GetAllAgents()
        {
            var agents = ConnectedAgents.Values.ToList();
            _logger.LogInformation("GetAllAgents: Trả về {Count} agents", agents.Count);
            return agents;
        }

        // ====================================
        // 2) Web gửi command xuống 1 agent cụ thể
        // ====================================
        // - request.AgentId: agent nào sẽ nhận command
        // - Nếu agent offline: báo ngược lại cho caller (web) biết
        // - Nếu agent online: gửi message "ReceiveCommand" cho đúng connectionId của agent
        public async Task SendCommand(CommandRequest request)
        {
            // Kiểm tra dữ liệu đầu vào
            if (request == null || string.IsNullOrWhiteSpace(request.AgentId))
            {
                _logger.LogWarning("SendCommand: request không hợp lệ (thiếu AgentId)");
                return;
            }

            var targetAgentId = request.AgentId;

            // Tìm agent và lấy connectionId
            if (!ConnectedAgents.TryGetValue(targetAgentId, out var agentInfo))
            {
                _logger.LogWarning(
                    "SendCommand: Agent {AgentId} không kết nối",
                    targetAgentId);

                // Báo lỗi cho client gọi hàm (caller = web)
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

            // Gửi command xuống đúng agent
            await Clients.Client(agentConnectionId).SendAsync(HubEvents.ExecuteCommand, request);
        }

        // ==================================
        // 3) Agent trả kết quả command cho server
        // ==================================
        // - result.AgentId: agent nào trả kết quả
        // - Broadcast sự kiện "CommandResultReceived" cho các client quan tâm (dashboard, log…)
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

            // TODO: sau này có thể lưu DB / log chi tiết tại đây

            // Broadcast kết quả cho tất cả client
            await Clients.All.SendAsync(HubEvents.CommandCompleted, result);
        }

        // ========================
        // Các hook lifecycle của Hub
        // ========================

        // Khi có client connect
        public override Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        // Khi client disconnect
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            // Xoá mọi AgentId có dùng connectionId này và broadcast AgentDisconnected
            foreach (var kvp in ConnectedAgents)
            {
                if (kvp.Value.ConnectionId == connectionId)
                {
                    ConnectedAgents.TryRemove(kvp.Key, out _);
                    _logger.LogInformation(
                        "OnDisconnected: xoá Agent {AgentId} (Conn {ConnectionId})",
                        kvp.Key, connectionId);
                    
                    // Broadcast AgentDisconnected event
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

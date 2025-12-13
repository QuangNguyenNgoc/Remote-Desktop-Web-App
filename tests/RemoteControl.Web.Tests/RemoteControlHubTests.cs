using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using RemoteControl.Shared.Models;
using RemoteControl.Shared.Constants;
using RemoteControl.Web.Hubs;

namespace RemoteControl.Web.Tests;

/// <summary>
/// Unit tests cho RemoteControlHub
/// Mô tả: Test các method chính của Hub với đầu vào/đầu ra rõ ràng
/// </summary>
public class RemoteControlHubTests
{
    private readonly Mock<ILogger<RemoteControlHub>> _loggerMock;
    private readonly Mock<IHubCallerClients> _clientsMock;
    private readonly Mock<HubCallerContext> _contextMock;
    private readonly Mock<IClientProxy> _allClientsMock;
    private readonly Mock<ISingleClientProxy> _callerMock;
    private readonly Mock<ISingleClientProxy> _singleClientMock;
    private readonly RemoteControlHub _hub;

    public RemoteControlHubTests()
    {
        _loggerMock = new Mock<ILogger<RemoteControlHub>>();
        _clientsMock = new Mock<IHubCallerClients>();
        _contextMock = new Mock<HubCallerContext>();
        _allClientsMock = new Mock<IClientProxy>();
        _callerMock = new Mock<ISingleClientProxy>();
        _singleClientMock = new Mock<ISingleClientProxy>();

        // Setup mock cho Clients.All
        _clientsMock.Setup(c => c.All).Returns(_allClientsMock.Object);
        // Setup mock cho Clients.Caller
        _clientsMock.Setup(c => c.Caller).Returns(_callerMock.Object);

        _hub = new RemoteControlHub(_loggerMock.Object);

        // Inject mock context và clients vào hub
        var hubType = typeof(Hub);
        hubType.GetProperty("Clients")!.SetValue(_hub, _clientsMock.Object);
        hubType.GetProperty("Context")!.SetValue(_hub, _contextMock.Object);
    }

    #region RegisterAgent Tests

    /// <summary>
    /// Test: RegisterAgent với AgentInfo hợp lệ
    /// Input: AgentInfo { AgentId = "agent-001", MachineName = "PC-001" }
    /// Expected: Broadcast "AgentRegistered" event với agent info
    /// </summary>
    [Fact]
    public async Task RegisterAgent_WithValidAgent_ShouldBroadcastAgentRegistered()
    {
        // Arrange - Chuẩn bị đầu vào
        var agent = new AgentInfo
        {
            AgentId = "agent-001",
            MachineName = "PC-001",
            IpAddress = "192.168.1.100",
            Status = AgentStatus.Online
        };

        _contextMock.Setup(c => c.ConnectionId).Returns("conn-123");

        // Act - Thực thi
        await _hub.RegisterAgent(agent);

        // Assert - Kiểm tra đầu ra
        _allClientsMock.Verify(
            c => c.SendCoreAsync(
                HubEvents.AgentConnected,
                It.Is<object[]>(o => o.Length == 1 && o[0] == agent),
                default),
            Times.Once,
            "Phải broadcast AgentConnected 1 lần với agent info");
    }

    /// <summary>
    /// Test: RegisterAgent với null agent
    /// Input: null
    /// Expected: Không broadcast, chỉ log warning
    /// </summary>
    [Fact]
    public async Task RegisterAgent_WithNullAgent_ShouldNotBroadcast()
    {
        // Arrange
        _contextMock.Setup(c => c.ConnectionId).Returns("conn-123");

        // Act
        await _hub.RegisterAgent(null!);

        // Assert - Không gọi SendAsync
        _allClientsMock.Verify(
            c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default),
            Times.Never,
            "Không được broadcast khi agent null");
    }

    /// <summary>
    /// Test: RegisterAgent với AgentId rỗng
    /// Input: AgentInfo { AgentId = "" }
    /// Expected: Không broadcast, chỉ log warning
    /// </summary>
    [Fact]
    public async Task RegisterAgent_WithEmptyAgentId_ShouldNotBroadcast()
    {
        // Arrange
        var agent = new AgentInfo { AgentId = "", MachineName = "PC" };
        _contextMock.Setup(c => c.ConnectionId).Returns("conn-123");

        // Act
        await _hub.RegisterAgent(agent);

        // Assert
        _allClientsMock.Verify(
            c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default),
            Times.Never,
            "Không được broadcast khi AgentId rỗng");
    }

    #endregion

    #region SendCommand Tests

    /// <summary>
    /// Test: SendCommand tới agent không tồn tại
    /// Input: CommandRequest { AgentId = "unknown-agent" }
    /// Expected: Gửi "CommandFailed" về cho caller
    /// </summary>
    [Fact]
    public async Task SendCommand_ToOfflineAgent_ShouldSendCommandFailed()
    {
        // Arrange
        var request = new CommandRequest
        {
            CommandId = "cmd-001",
            AgentId = "unknown-agent",
            Type = CommandType.ListProcesses
        };

        _contextMock.Setup(c => c.ConnectionId).Returns("web-conn-456");

        // Act
        await _hub.SendCommand(request);

        // Assert - Phải gửi CommandFailed về caller
        _callerMock.Verify(
            c => c.SendCoreAsync(
                "CommandFailed",
                It.Is<object[]>(o => o.Length == 2),
                default),
            Times.Once,
            "Phải gửi CommandFailed khi agent offline");
    }

    /// <summary>
    /// Test: SendCommand với request null
    /// Input: null
    /// Expected: Không gửi command, chỉ log warning
    /// </summary>
    [Fact]
    public async Task SendCommand_WithNullRequest_ShouldNotSend()
    {
        // Act
        await _hub.SendCommand(null!);

        // Assert
        _clientsMock.Verify(
            c => c.Client(It.IsAny<string>()),
            Times.Never,
            "Không được gửi command khi request null");
    }

    /// <summary>
    /// Test: SendCommand với AgentId rỗng
    /// Input: CommandRequest { AgentId = "" }
    /// Expected: Không gửi command
    /// </summary>
    [Fact]
    public async Task SendCommand_WithEmptyAgentId_ShouldNotSend()
    {
        // Arrange
        var request = new CommandRequest
        {
            CommandId = "cmd-002",
            AgentId = "",
            Type = CommandType.CaptureScreen
        };

        // Act
        await _hub.SendCommand(request);

        // Assert
        _clientsMock.Verify(
            c => c.Client(It.IsAny<string>()),
            Times.Never,
            "Không được gửi command khi AgentId rỗng");
    }

    #endregion

    #region SendResult Tests

    /// <summary>
    /// Test: SendResult với result hợp lệ
    /// Input: CommandResult { CommandId = "cmd-001", Success = true }
    /// Expected: Broadcast "CommandResultReceived" cho tất cả clients
    /// </summary>
    [Fact]
    public async Task SendResult_WithValidResult_ShouldBroadcast()
    {
        // Arrange
        var result = new CommandResult
        {
            CommandId = "cmd-001",
            AgentId = "agent-001",
            Success = true,
            Message = "Đã thực thi thành công"
        };

        // Act
        await _hub.SendResult(result);

        // Assert
        _allClientsMock.Verify(
            c => c.SendCoreAsync(
                HubEvents.CommandCompleted,
                It.Is<object[]>(o => o.Length == 1 && o[0] == result),
                default),
            Times.Once,
            "Phải broadcast CommandCompleted cho tất cả clients");
    }

    /// <summary>
    /// Test: SendResult với null
    /// Input: null
    /// Expected: Không broadcast
    /// </summary>
    [Fact]
    public async Task SendResult_WithNullResult_ShouldNotBroadcast()
    {
        // Act
        await _hub.SendResult(null!);

        // Assert
        _allClientsMock.Verify(
            c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default),
            Times.Never,
            "Không được broadcast khi result null");
    }

    /// <summary>
    /// Test: SendResult với AgentId rỗng
    /// Input: CommandResult { AgentId = "" }
    /// Expected: Không broadcast
    /// </summary>
    [Fact]
    public async Task SendResult_WithEmptyAgentId_ShouldNotBroadcast()
    {
        // Arrange
        var result = new CommandResult
        {
            CommandId = "cmd-003",
            AgentId = "",
            Success = false
        };

        // Act
        await _hub.SendResult(result);

        // Assert
        _allClientsMock.Verify(
            c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default),
            Times.Never,
            "Không được broadcast khi AgentId rỗng");
    }

    #endregion

    #region GetAllAgents Tests

    /// <summary>
    /// Test: GetAllAgents trả về List (không null)
    /// Expected: List không null, có thể rỗng hoặc chứa agents
    /// </summary>
    [Fact]
    public void GetAllAgents_ShouldReturnNonNullList()
    {
        // Act
        var result = _hub.GetAllAgents();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<List<AgentInfo>>(result);
    }

    /// <summary>
    /// Test: GetAllAgents sau khi đã register agent mới
    /// Expected: Agent mới phải có trong list
    /// </summary>
    [Fact]
    public async Task GetAllAgents_AfterRegisterAgent_ShouldContainNewAgent()
    {
        // Arrange - Tạo agent với ID unique
        var uniqueId = $"agent-{Guid.NewGuid()}";
        var agent = new AgentInfo
        {
            AgentId = uniqueId,
            MachineName = "TEST-PC-UNIQUE",
            IpAddress = "10.0.0.99"
        };
        _contextMock.Setup(c => c.ConnectionId).Returns($"conn-{Guid.NewGuid()}");

        // Act
        await _hub.RegisterAgent(agent);
        var result = _hub.GetAllAgents();

        // Assert - Agent mới phải có trong list
        var foundAgent = result.FirstOrDefault(a => a.AgentId == uniqueId);
        Assert.NotNull(foundAgent);
        Assert.Equal("TEST-PC-UNIQUE", foundAgent.MachineName);
        Assert.Equal(AgentStatus.Online, foundAgent.Status);
    }

    #endregion
}

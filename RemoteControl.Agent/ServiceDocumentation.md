# Service Documentation: SignalR & Power

This document provides a detailed overview of the `SignalRClientService` and `PowerService` in the `RemoteControl.Agent` application.

## 1. SignalRClientService

**Class**: `RemoteControl.Agent.Services.SignalRClientService`
**Purpose**: Manages the persistent, real-time WebSocket connection to the central Hub. It handles identification, command reception, and heartbeat monitoring.

### Key Variables

| Variable                   | Type                  | Description                                                                              |
| :------------------------- | :-------------------- | :--------------------------------------------------------------------------------------- |
| `_hubConnection`           | `HubConnection`       | The main client object from `Microsoft.AspNetCore.SignalR.Client`. Maintains the socket. |
| `_commandHandler`          | `CommandHandler`      | Reference to the handler that executes logic when the Hub sends a command.               |
| `_heartbeatTimer`          | `System.Timers.Timer` | Fires every 10 seconds to send a "Heartbeat" message to the server.                      |
| `_agentId`                 | `string`              | Unique identifier (currently `Environment.MachineName`).                                 |
| `OnStatusChanged`          | `Action<string>`      | Event used to push log messages to the UI thread.                                        |
| `OnConnectionStateChanged` | `Action<string>`      | Event used to notify the UI about connection state (Connected, Reconnecting, Closed).    |

### Core Functions & Workflow

1.  **`ConnectAsync()`**

    - **Logic**: Builds the `HubConnection` to `http://localhost:5048/remotehub`. Configures `WithAutomaticReconnect`.
    - **Workflow**: Starts connection -> Triggers `RegisterAgentAsync` on success -> Starts `_heartbeatTimer`.

2.  **`RegisterAgentAsync()`**

    - **Logic**: Creates an `AgentInfo` object with Machine Name, IP, and OS Version.
    - **Workflow**: Calls `await _hubConnection.InvokeAsync("RegisterAgent", agentInfo)`.

3.  **`RegisterHandlers()`**

    - **Logic**: Subscribes to the `ExecuteCommand` event.
    - **Workflow**: When Hub sends `ExecuteCommand(CommandRequest)`, this method calls `_commandHandler.HandleCommand(request)` and then returns the result via `SendResultAsync`.

4.  **`SendHeartbeat()`**
    - **Logic**: Called by `_heartbeatTimer`.
    - **Workflow**: Sends `InvokeAsync("SendHeartbeat", _agentId)` to prove the agent is online.

### How to Test (SignalR)

1.  **Run the Server**: Ensure the `RemoteControl.Web` project is running.
2.  **Run the Agent**: Start `RemoteControl.Agent`.
3.  **Visual Verification**:
    - Observe the "Status" bar in `AgentDebugForm`. It should switch from "Connecting..." to "Connected".
    - The Window Title should show `[Connected]`.
4.  **Log Verification**:
    - Check Server logs for "Agent [Name] registered".
    - Check Server logs for "Heartbeat received".

---

## 2. PowerService

**Class**: `RemoteControl.Agent.Services.PowerService`
**Purpose**: Provides low-level system control to Shutdown, Restart, Sleep, or Lock the machine.

### Key Variables / Imports

| Variable          | Type          | Description                                                       |
| :---------------- | :------------ | :---------------------------------------------------------------- |
| `LockWorkStation` | `extern bool` | Imported from `user32.dll`. Locks the Windows session.            |
| `SetSuspendState` | `extern bool` | Imported from `PowrProf.dll`. Puts machine to Sleep or Hibernate. |

### Core Functions & Workflow

1.  **`Lock()`**

    - **Logic**: Calls `LockWorkStation()`.
    - **Effect**: The screen immediately locks (Win+L key equivalent).

2.  **`Sleep()`**

    - **Logic**: Calls `SetSuspendState(false, true, false)`.
    - **Effect**: The computer enters low-power Sleep mode.

3.  **`Shutdown()`**

    - **Logic**: Executes `Process.Start("shutdown", "/s /t 0")`.
    - **Effect**: Forces a system shutdown immediately (0 delay).

4.  **`Restart()`**
    - **Logic**: Executes `Process.Start("shutdown", "/r /t 0")`.
    - **Effect**: Forces a system restart immediately.

### How to Test (Power)

> **⚠️ WARNING**: Testing Shutdown/Restart/Sleep will disrupt your current session!

1.  **Open Agent Console**: Run the `RemoteControl.Agent` app.
2.  **Navigate to "Power" Tab**: This tab contains the control buttons.
3.  **Test Lock (Safe)**:
    - Click **"Lock Workstation"**.
    - Confirm the confirmation dialog.
    - **Result**: The screen should lock immediately. Unlock to continue.
4.  **Test Sleep**:
    - Click **"Sleep / Suspend"**.
    - Confirm.
    - **Result**: PC should go to sleep. Wake it up.
5.  **Test Shutdown/Restart**:
    - Click corresponding button.
    - **Result**: PC will reboot/shutdown. **Save all work before clicking!**

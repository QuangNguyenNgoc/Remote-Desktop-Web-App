# RemoteControl Agent - Service Management Scripts
# PowerShell scripts để install/uninstall Windows Service

# ====== IMPORTANT ======
# 1. Run as Administrator
# 2. Modify $AgentPath to point to your Agent.exe location
# 3. Use at your own risk

# ====== Variables ======
$ServiceName = "RemoteControlAgent"
$DisplayName = "Remote Control Agent"
$Description = "Remote Control Agent Service for IT Management"

# Get the directory where this script is located
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$AgentPath = Join-Path (Split-Path -Parent $ScriptDir) "bin\Release\net10.0-windows\win-x64\publish\RemoteControl.Agent.exe"

# ====== Helper Functions ======
function Test-Admin {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Show-Menu {
    Write-Host ""
    Write-Host "======================================" -ForegroundColor Cyan
    Write-Host "  RemoteControl Agent Service Manager" -ForegroundColor Cyan
    Write-Host "======================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "1. Install Service"
    Write-Host "2. Uninstall Service"
    Write-Host "3. Start Service"
    Write-Host "4. Stop Service"
    Write-Host "5. Check Status"
    Write-Host "6. Set Custom Agent Path"
    Write-Host "Q. Quit"
    Write-Host ""
}

function Install-AgentService {
    if (-not (Test-Path $AgentPath)) {
        Write-Host "ERROR: Agent not found at: $AgentPath" -ForegroundColor Red
        Write-Host "Please build the agent first or set custom path (option 6)" -ForegroundColor Yellow
        return
    }

    Write-Host "Installing service..." -ForegroundColor Yellow
    
    # Create service
    $binPath = "`"$AgentPath`" --mode=service"
    sc.exe create $ServiceName binPath= $binPath start= auto DisplayName= $DisplayName
    
    # Set description
    sc.exe description $ServiceName $Description
    
    # Set recovery options (restart on failure)
    sc.exe failure $ServiceName reset= 86400 actions= restart/60000/restart/60000/restart/60000
    
    Write-Host "Service installed successfully!" -ForegroundColor Green
    Write-Host "Run 'sc.exe start $ServiceName' to start the service" -ForegroundColor Cyan
}

function Uninstall-AgentService {
    Write-Host "Stopping service..." -ForegroundColor Yellow
    sc.exe stop $ServiceName 2>$null
    
    Start-Sleep -Seconds 2
    
    Write-Host "Removing service..." -ForegroundColor Yellow
    sc.exe delete $ServiceName
    
    Write-Host "Service uninstalled successfully!" -ForegroundColor Green
}

function Start-AgentService {
    Write-Host "Starting service..." -ForegroundColor Yellow
    sc.exe start $ServiceName
    Write-Host "Service started!" -ForegroundColor Green
}

function Stop-AgentService {
    Write-Host "Stopping service..." -ForegroundColor Yellow
    sc.exe stop $ServiceName
    Write-Host "Service stopped!" -ForegroundColor Green
}

function Check-ServiceStatus {
    Write-Host "Checking service status..." -ForegroundColor Yellow
    sc.exe query $ServiceName
}

# ====== Main ======
if (-not (Test-Admin)) {
    Write-Host "ERROR: This script requires Administrator privileges!" -ForegroundColor Red
    Write-Host "Please run PowerShell as Administrator and try again." -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "Current Agent Path: $AgentPath" -ForegroundColor Gray

do {
    Show-Menu
    $choice = Read-Host "Select option"
    
    switch ($choice) {
        "1" { Install-AgentService }
        "2" { Uninstall-AgentService }
        "3" { Start-AgentService }
        "4" { Stop-AgentService }
        "5" { Check-ServiceStatus }
        "6" { 
            $AgentPath = Read-Host "Enter full path to Agent.exe"
            Write-Host "Agent path set to: $AgentPath" -ForegroundColor Green
        }
        "Q" { break }
        "q" { break }
        default { Write-Host "Invalid option" -ForegroundColor Red }
    }
    
    if ($choice -ne "Q" -and $choice -ne "q") {
        Write-Host ""
        Read-Host "Press Enter to continue"
    }
} while ($choice -ne "Q" -and $choice -ne "q")

Write-Host "Goodbye!" -ForegroundColor Cyan

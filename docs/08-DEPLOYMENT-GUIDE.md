# Deployment Guide - Remote Control Desktop

## Deployment Architecture

### Production Environment (Planned)

```
┌─────────────────────────────────────┐
│      Cloud Server (Azure/AWS)       │
│  ┌───────────────────────────────┐  │
│  │   Docker Container: Web       │  │
│  │   Port 80 (HTTP) / 443 (HTTPS │  │
│  └───────────────────────────────┘  │
└─────────────────────────────────────┘
              ▲
              │ Internet
              │
       ┌──────┴──────┐
       │             │
 Agent Machine   Agent Machine
 (Desktop)       (Desktop)
```

---

## Deployment Option 1: Docker Container (Recommended)

### Prerequisites
- Docker Desktop installed
- Docker Compose (included with Docker Desktop)

### Build Production Image

```dockerfile
# docker/Dockerfile.web
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["RemoteControl.Web/RemoteControl.Web.csproj", "RemoteControl.Web/"]
COPY ["RemoteControl.Shared/RemoteControl.Shared.csproj", "RemoteControl.Shared/"]

# Restore packages
RUN dotnet restore "RemoteControl.Web/RemoteControl.Web.csproj"

# Copy all source
COPY . .

WORKDIR "/src/RemoteControl.Web"

# Build
RUN dotnet build "RemoteControl.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RemoteControl.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RemoteControl.Web.dll"]
```

### Build Commands

```powershell
# Build image
docker build -t remotecontrol-web:latest -f docker/Dockerfile.web .

# Run container
docker run -d -p 5000:80 --name remotecontrol-web remotecontrol-web:latest

# Verify
docker ps
docker logs remotecontrol-web
```

### Docker Compose (Recommended)

```yaml
# docker-compose.yml
version: '3.8'

services:
  web:
    build:
      context: .
      dockerfile: docker/Dockerfile.web
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:80;https://+:443
    volumes:
      - ./logs:/app/logs
    restart: unless-stopped

  # Future: Database service
  # db:
  #   image: postgres:15
  #   environment:
  #     POSTGRES_DB: remotecontrol
```

**Deploy**:
```powershell
docker-compose up -d
docker-compose logs -f web
```

---

## Deployment Option 2: Direct Deployment (IIS/Kestrel)

### Prerequisites
- Windows Server 2019+
- .NET 10.0 Runtime installed
- IIS (optional)

### Publish Application

```powershell
# Navigate to project root
cd RemoteControlProject

# Publish Web app
dotnet publish RemoteControl.Web/RemoteControl.Web.csproj -c Release -o ./publish

# Output folder: ./publish/
```

### Run with Kestrel (Standalone)

```powershell
cd publish

# Run directly
dotnet RemoteControl.Web.dll

# With custom URLs
dotnet RemoteControl.Web.dll --urls "http://*:5000;https://*:5001"
```

### Configure IIS (Windows)

1. **Install IIS** + **ASP.NET Core Hosting Bundle**
   ```powershell
   # Download from: https://dotnet.microsoft.com/download/dotnet/10.0
   ```

2. **Create IIS Site**:
   - Open IIS Manager
   - Add Website: `RemoteControlWeb`
   - Physical path: `C:\inetpub\RemoteControl\publish`
   - Bindings: Port 80

3. **Configure Application Poll**:
   - No Managed Code
   - Identity: ApplicationPoolIdentity

---

## Configuration Management

### Environment Variables

**Production Settings**:
```powershell
# Set environment
setx ASPNETCORE_ENVIRONMENT "Production"

# Database connection (if used)
setx ConnectionStrings__DefaultConnection "Server=...;Database=..."

# SignalR Hub URL
setx SignalR__HubUrl "/remotehub"
```

**appsettings.Production.json**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "yourdomain.com",
  "SignalR": {
    "MaxMessageSize": 32768
  }
}
```

### Secrets Management

**❌ Don't commit secrets**:
```json
// appsettings.json - BAD
{
  "ApiKey": "sk-1234567890"  // Never commit this!
}
```

**✅ Use User Secrets** (Development):
```powershell
cd RemoteControl.Web
dotnet user-secrets init
dotnet user-secrets set "ApiKey" "sk-1234567890"
```

**✅ Use Environment Variables** (Production):
```powershell
setx ApiKey "sk-1234567890"
```

---

## Agent Distribution

### Build Agent Executable

```powershell
# Publish as single-file executable
dotnet publish RemoteControl.Agent/RemoteControl.Agent.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -o ./agent-dist

# Output: agent-dist/RemoteControl.Agent.exe (self-contained)
```

### Agent Configuration

**appsettings.json** (in same folder as .exe):
```json
{
  "HubUrl": "https://your-server.com/remotehub",
  "AgentId": "",  // Auto-generated if empty
  "ReconnectIntervalSeconds": 10
}
```

### Distribute to Clients

1. Copy `RemoteControl.Agent.exe` + `appsettings.json` to target machines
2. Run as Administrator (for system-level operations)
3. Optionally set as Windows Service

**Create Windows Service** (Optional):
```powershell
sc.exe create "RemoteControlAgent" binPath= "C:\Path\To\RemoteControl.Agent.exe"
sc.exe start "RemoteControlAgent"
```

---

## SSL/TLS Configuration

### Development (Self-Signed)

```powershell
# Trust self-signed certificate
dotnet dev-certs https --trust
```

### Production (Let's Encrypt - Linux)

```bash
# Install Certbot
sudo apt install certbot

# Get certificate
sudo certbot certonly --standalone -d yourdomain.com

# Configure Kestrel
```

**appset tings.Production.json**:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://*:443",
        "Certificate": {
          "Path": "/etc/letsencrypt/live/yourdomain.com/fullchain.pem",
          "KeyPath": "/etc/letsencrypt/live/yourdomain.com/privkey.pem"
        }
      }
    }
  }
}
```

---

## Monitoring & Logging

### Application Logs

**Configure Logging**:
```csharp
// Program.cs
builder.Logging.AddFile("logs/app-{Date}.log");  // Install Serilog.Extensions.Logging.File
```

**Log Location**:
- Development: Console output
- Production: `./logs/app-2024-12-02.log`

### Health Checks (Planned)

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddCheck("SignalRHub", () => HealthCheckResult.Healthy());

app.MapHealthChecks("/health");
```

**Endpoint**: `http://your-server.com/health` → Should return "Healthy"

### Monitoring Tools (Planned)

- **Application Insights** (Azure)
- **Prometheus + Grafana** (Self-hosted)
- **Uptime Monitoring**: UptimeRobot, Pingdom

---

## Backup & Recovery

### Backup Strategy (If using database)

```powershell
# Backup database daily
# pg_dump -U postgres remotecontrol > backup_$(date +%Y%m%d).sql
```

### Configuration Backup

```powershell
# Backup appsettings
Copy-Item appsettings.Production.json backups/appsettings.$(Get-Date -Format "yyyyMMdd").json
```

---

## Scaling Considerations

### Single Server (Current)

```
All agents → Single Web Server → Single Hub instance
```

**Limitations**:
- Max ~10,000 concurrent WebSocket connections
- Single point of failure

### Load Balanced (Future)

```
          ┌─── Web Server 1 ───┐
 Agents → │    Load Balancer   │ → Redis (SignalR backplane)
          └─── Web Server 2 ───┘
```

**Required**:
- Redis for SignalR backplane
- Sticky sessions (or stateless Hub)
- Shared storage for logs

**Configuration**:
```csharp
// Program.cs
builder.Services.AddSignalR()
    .AddStackExchangeRedis("redis://localhost:6379");
```

---

## Pre-Deployment Checklist

**Code**:
- [ ] All tests passing
- [ ] No compiler warnings
- [ ] Secrets removed from appsettings
- [ ] Production URLs configured

**Infrastructure**:
- [ ] Server provisioned (Cloud/On-prem)
- [ ] SSL certificate obtained
- [ ] Firewall rules configured (Allow port 80/443)
- [ ] Database created (if applicable)

**Configuration**:
- [ ] Environment variables set
- [ ] appsettings.Production.json reviewed
- [ ] Logging configured
- [ ] Health check endpoint tested

**Security**:
- [ ] HTTPS enforced
- [ ] Authentication enabled (if implemented)
- [ ] CORS configured properly
- [ ] Rate limiting enabled (if needed)

**Monitoring**:
- [ ] Health check endpoint live
- [ ] Logging working
- [ ] Alerts configured

---

## Rollback Plan

**If Deployment Fails**:

1. **Stop new version**:
   ```powershell
   docker-compose down
   ```

2. **Restore previous version**:
   ```powershell
   docker-compose up -d --build
   ```

3. **Restore database** (if applicable):
   ```powershell
   # psql -U postgres remotecontrol < backup_20241201.sql
   ```

---

## Post-Deployment Verification

```powershell
# 1. Check app is running
curl https://your-server.com/health

# 2. Test SignalR connection
# Open browser: https://your-server.com
# Check browser console for connection errors

# 3. Test agent connection
.\RemoteControl.Agent.exe

# 4. Monitor logs
docker-compose logs -f web
```

---

**Cập Nhật**: 02/12/2024  
**Phiên Bản**: 1.0

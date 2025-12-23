# Issue #26: [DevOps] Dockerfile & Docker Compose

### 🎯 Mục Tiêu

Containerize ứng dụng với Docker để dễ deploy

### ✅ Checklist

**RemoteControl.Web Dockerfile:**
- [x] Tạo `RemoteControl.Web/Dockerfile`
- [x] Multi-stage build: restore → build → publish
- [x] Base image: `mcr.microsoft.com/dotnet/aspnet:9.0`
- [x] Expose port 5048
- [x] Set ASPNETCORE_URLS environment variable
- [x] Health check endpoint `/health`

**RemoteControl.Agent Dockerfile:**
- [ ] Tạo `RemoteControl.Agent/Dockerfile`
- [x] ~~Windows container (vì dùng System.Drawing, PerformanceCounter)~~ **Không cần - Agent phải chạy native**

**Docker Compose:**
- [x] Tạo `docker-compose.yml` ở root
- [x] Service: `web` (RemoteControl.Web)
- [ ] Volume mount cho config files
- [ ] Network configuration
- [x] Health checks

**Documentation:**
- [x] Update README với Docker instructions
- [x] Update BUILD-AND-PUBLISH-GUIDE.md
- [ ] Add docker-compose.override.yml for dev

### 🔗 Dependencies

- ✅ MVP hoàn thành
- ⏳ Làm trước khi CI/CD (#27)

### 📝 Sample Dockerfile

```dockerfile
# RemoteControl.Web/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5048

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["RemoteControl.Web/RemoteControl.Web.csproj", "RemoteControl.Web/"]
COPY ["RemoteControl.Shared/RemoteControl.Shared.csproj", "RemoteControl.Shared/"]
RUN dotnet restore "RemoteControl.Web/RemoteControl.Web.csproj"
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5048
ENTRYPOINT ["dotnet", "RemoteControl.Web.dll"]
```

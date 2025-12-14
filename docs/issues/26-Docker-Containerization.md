# Issue #26: [DevOps] Dockerfile & Docker Compose

### 🎯 Mục Tiêu

Containerize ứng dụng với Docker để dễ deploy

### ✅ Checklist

**RemoteControl.Web Dockerfile:**
- [ ] Tạo `RemoteControl.Web/Dockerfile`
- [ ] Multi-stage build: restore → build → publish
- [ ] Base image: `mcr.microsoft.com/dotnet/aspnet:10.0`
- [ ] Expose port 5048
- [ ] Set ASPNETCORE_URLS environment variable

**RemoteControl.Agent Dockerfile:**
- [ ] Tạo `RemoteControl.Agent/Dockerfile`
- [ ] Windows container (vì dùng System.Drawing, PerformanceCounter)
- [ ] Hoặc Linux container với workaround

**Docker Compose:**
- [ ] Tạo `docker-compose.yml` ở root
- [ ] Service: `web` (RemoteControl.Web)
- [ ] Volume mount cho config files
- [ ] Network configuration
- [ ] Health checks

**Documentation:**
- [ ] Update README với Docker instructions
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

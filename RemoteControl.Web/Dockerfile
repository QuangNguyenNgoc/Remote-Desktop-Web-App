# RemoteControl.Web Dockerfile
# Multi-stage build for optimized production image

# ===== Stage 1: Build =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files first (for layer caching)
COPY ["RemoteControl.Web/RemoteControl.Web.csproj", "RemoteControl.Web/"]
COPY ["RemoteControl.Shared/RemoteControl.Shared.csproj", "RemoteControl.Shared/"]

# Restore dependencies
RUN dotnet restore "RemoteControl.Web/RemoteControl.Web.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/RemoteControl.Web"
RUN dotnet build -c Release -o /app/build

# ===== Stage 2: Publish =====
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ===== Stage 3: Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Expose SignalR port
EXPOSE 5048

# Copy published files
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5048
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5048/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "RemoteControl.Web.dll"]

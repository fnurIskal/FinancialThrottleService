# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["FinancialThrottle.Apii/FinancialThrottle.Apii.csproj", "FinancialThrottle.Apii/"]
COPY ["FinancialThrottle.Worker/FinancialThrottle.Worker.csproj", "FinancialThrottle.Worker/"]
COPY ["FinancialThrottleService.Application/FinancialThrottleService.Application.csproj", "FinancialThrottleService.Application/"]
COPY ["FinancialThrottleService.Domain/FinancialThrottleService.Domain.csproj", "FinancialThrottleService.Domain/"]
COPY ["FinancialThrottleService.Infrastructure/FinancialThrottleService.Infrastructure.csproj", "FinancialThrottleService.Infrastructure/"]

# Restore
RUN dotnet restore "FinancialThrottle.Apii/FinancialThrottle.Apii.csproj"

# Copy source
COPY . .

# Build
RUN dotnet build "FinancialThrottle.Apii/FinancialThrottle.Apii.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "FinancialThrottle.Apii/FinancialThrottle.Apii.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Ports
EXPOSE 5059 7176 5000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:5059/api/status || exit 1

ENTRYPOINT ["dotnet", "FinancialThrottle.Apii.dll"]

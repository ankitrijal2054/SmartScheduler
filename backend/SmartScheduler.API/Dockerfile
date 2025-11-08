# Stage 1: Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder

WORKDIR /src

# Copy project files
COPY ["SmartScheduler.API/SmartScheduler.API.csproj", "SmartScheduler.API/"]
COPY ["SmartScheduler.Application/SmartScheduler.Application.csproj", "SmartScheduler.Application/"]
COPY ["SmartScheduler.Domain/SmartScheduler.Domain.csproj", "SmartScheduler.Domain/"]
COPY ["SmartScheduler.Infrastructure/SmartScheduler.Infrastructure.csproj", "SmartScheduler.Infrastructure/"]

# Restore dependencies for API project
RUN dotnet restore "SmartScheduler.API/SmartScheduler.API.csproj"

# Copy entire source code
COPY . .

# Build API project in Release mode
RUN dotnet build "SmartScheduler.API/SmartScheduler.API.csproj" -c Release -o /app/build

# Publish application
RUN dotnet publish "SmartScheduler.API/SmartScheduler.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime

# Install curl for health checks
RUN apk add --no-cache curl

WORKDIR /app

# Copy published application from builder stage
COPY --from=builder /app/publish .

# Expose port (App Runner expects 8080)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080

# Health check
HEALTHCHECK --interval=10s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "SmartScheduler.API.dll"]


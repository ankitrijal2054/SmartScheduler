using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Application.Services;
using SmartScheduler.Infrastructure.Persistence;
using SmartScheduler.Infrastructure.Repositories;
using SmartScheduler.Infrastructure.Services;

namespace SmartScheduler.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering Infrastructure layer services in the DI container.
/// </summary>
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext with PostgreSQL
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
            // Suppress PendingModelChangesWarning as an error - allow app to start for health checks
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        // Register repositories
        services.AddScoped<IContractorRepository, ContractorRepository>();
        services.AddScoped<IAssignmentRepository, AssignmentRepository>();
        services.AddScoped<IDispatcherContractorListRepository, DispatcherContractorListRepository>();

        // Register external service clients
        services.AddHttpClient<GoogleMapsGeocodingService>();
        services.AddScoped<IGeocodingService, GoogleMapsGeocodingService>();

        // Register Distance Service with Google Maps API integration and Redis caching
        var googleMapsApiKey = configuration["GoogleMaps:ApiKey"];
        var redisConnectionString = configuration["Redis:ConnectionString"];

        // Only register distance services if configuration is available
        if (!string.IsNullOrEmpty(googleMapsApiKey) && !string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddHttpClient<GoogleMapsDistanceService>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5));

            services.AddScoped(provider =>
                new GoogleMapsDistanceService(
                    provider.GetRequiredService<HttpClient>(),
                    googleMapsApiKey,
                    provider.GetRequiredService<ILogger<GoogleMapsDistanceService>>()
                )
            );

            // Configure Redis distributed cache
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
            });

            // Register CachedDistanceService as the primary IDistanceService implementation
            services.AddScoped<IDistanceService>(provider =>
                new CachedDistanceService(
                    provider.GetRequiredService<GoogleMapsDistanceService>(),
                    provider.GetRequiredService<IDistributedCache>(),
                    provider.GetRequiredService<ILogger<CachedDistanceService>>()
                )
            );
        }

        return services;
    }
}


using MediatR;
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
        services.AddScoped<IReviewRepository, ReviewRepository>();

        // Register MediatR handlers from Infrastructure assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(InfrastructureServiceExtensions).Assembly));

        // Register application services
        services.AddScoped<IRatingAggregationService, RatingAggregationService>();

        // Register external service clients
        services.AddHttpClient<GoogleMapsGeocodingService>();
        services.AddScoped<IGeocodingService, GoogleMapsGeocodingService>();

        // Register Distance Service with Google Maps API integration and Redis caching
        var googleMapsApiKey = configuration["GoogleMaps:ApiKey"] ?? "dummy-key";
        var redisConnectionString = configuration["Redis:ConnectionString"];

        // Always register GoogleMapsDistanceService (will use Haversine fallback if API key invalid)
        services.AddHttpClient<GoogleMapsDistanceService>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10); // 10 second timeout for Google Maps API calls
            });

        services.AddScoped(provider =>
            new GoogleMapsDistanceService(
                provider.GetRequiredService<HttpClient>(),
                googleMapsApiKey,
                provider.GetRequiredService<ILogger<GoogleMapsDistanceService>>()
            )
        );

        // Register Redis cache if connection string is available
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            try
            {
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
            catch
            {
                // If Redis fails to configure, fall back to direct GoogleMapsDistanceService
                services.AddScoped<IDistanceService>(provider =>
                    provider.GetRequiredService<GoogleMapsDistanceService>());
            }
        }
        else
        {
            // No Redis configured, use GoogleMapsDistanceService directly
            services.AddScoped<IDistanceService>(provider =>
                provider.GetRequiredService<GoogleMapsDistanceService>());
        }

        return services;
    }
}


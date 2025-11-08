using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        // Register external service clients
        services.AddHttpClient<GoogleMapsGeocodingService>();
        services.AddScoped<IGeocodingService, GoogleMapsGeocodingService>();

        return services;
    }
}


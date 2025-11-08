using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartScheduler.Infrastructure.Persistence;

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

        // Future: Register repositories
        // services.AddScoped<IContractorRepository, ContractorRepository>();
        // services.AddScoped<IJobRepository, JobRepository>();

        // Future: Register external service clients
        // services.AddHttpClient<IGoogleMapsClient, GoogleMapsClient>();
        // services.AddSingleton<IEmailService, AwsSesEmailService>();

        return services;
    }
}


using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        // Future: Register DbContext
        // var connectionString = configuration.GetConnectionString("DefaultConnection");
        // services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        // Future: Register repositories
        // services.AddScoped<IContractorRepository, ContractorRepository>();
        // services.AddScoped<IJobRepository, JobRepository>();

        // Future: Register external service clients
        // services.AddHttpClient<IGoogleMapsClient, GoogleMapsClient>();
        // services.AddSingleton<IEmailService, AwsSesEmailService>();

        return services;
    }
}


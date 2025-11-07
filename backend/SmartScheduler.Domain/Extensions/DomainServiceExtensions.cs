using Microsoft.Extensions.DependencyInjection;

namespace SmartScheduler.Domain.Extensions;

/// <summary>
/// Extension methods for registering Domain layer services in the DI container.
/// </summary>
public static class DomainServiceExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Domain layer contains only business logic and entities
        // Services are registered in Application layer
        return services;
    }
}


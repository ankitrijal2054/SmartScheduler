using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using SmartScheduler.Application.Services;

namespace SmartScheduler.Application.Extensions;

/// <summary>
/// Extension methods for registering Application layer services in the DI container.
/// </summary>
public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR for CQRS pattern
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly));

        // Register FluentValidation validators
        services.AddValidatorsFromAssemblyContaining<AssemblyMarker>();

        // Register authentication services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHashingService, PasswordHashingService>();

        // Register authorization services
        services.AddScoped<IAuthorizationService, AuthorizationService>();

        // Register contractor services
        services.AddScoped<IContractorService, ContractorService>();

        // Register availability services
        services.AddScoped<IAvailabilityService, AvailabilityService>();

        // Register scoring services (Story 2.4)
        services.AddScoped<IScoringService, ScoringService>();

        // Future: Register AutoMapper for DTOs
        // services.AddAutoMapper(typeof(ApplicationServiceExtensions).Assembly);

        return services;
    }
}


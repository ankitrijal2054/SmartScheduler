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
        // Register MediatR for CQRS pattern and domain events
        // Note: Infrastructure handlers are registered separately in InfrastructureServiceExtensions
        // This is called first, then Infrastructure handlers are added after
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

        // Register email services (Story 4.5)
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<IEmailService, EmailService>();

        // Future: Register AutoMapper for DTOs
        // services.AddAutoMapper(typeof(ApplicationServiceExtensions).Assembly);

        return services;
    }
}


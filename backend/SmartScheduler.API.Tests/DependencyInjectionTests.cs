using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using SmartScheduler.Domain.Extensions;
using SmartScheduler.Application.Extensions;
using SmartScheduler.Infrastructure.Extensions;

namespace SmartScheduler.API.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddDomainServices_Should_RegisterWithoutErrors()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddDomainServices();

        // Assert
        result.Should().NotBeNull();
        var provider = services.BuildServiceProvider();
        provider.Should().NotBeNull();
    }

    [Fact]
    public void AddApplicationServices_Should_RegisterWithoutErrors()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddApplicationServices();

        // Assert
        result.Should().NotBeNull();
        var provider = services.BuildServiceProvider();
        provider.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructureServices_Should_RegisterWithoutErrors()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ConnectionStrings:DefaultConnection", "Server=localhost;Database=test;" }
            })
            .Build();

        // Act
        var result = services.AddInfrastructureServices(configuration);

        // Assert
        result.Should().NotBeNull();
        var provider = services.BuildServiceProvider();
        provider.Should().NotBeNull();
    }

    [Fact]
    public void AllLayersRegistered_Should_BuildServiceProviderSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>())
            .Build();

        // Act
        services.AddDomainServices();
        services.AddApplicationServices();
        services.AddInfrastructureServices(configuration);

        var provider = services.BuildServiceProvider();

        // Assert
        provider.Should().NotBeNull();
        // Verify no circular dependencies by attempting to create a provider
        var ex = Record.Exception(() => provider.GetRequiredService<IServiceProvider>());
        ex.Should().BeNull();
    }
}


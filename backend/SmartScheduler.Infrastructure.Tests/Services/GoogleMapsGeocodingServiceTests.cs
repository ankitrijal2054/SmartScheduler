using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SmartScheduler.Infrastructure.Services;

namespace SmartScheduler.Infrastructure.Tests.Services;

/// <summary>
/// Unit tests for GoogleMapsGeocodingService.
/// </summary>
public class GoogleMapsGeocodingServiceTests
{
    private readonly Mock<ILogger<GoogleMapsGeocodingService>> _loggerMock;
    private readonly GoogleMapsGeocodingService _service;

    public GoogleMapsGeocodingServiceTests()
    {
        _loggerMock = new Mock<ILogger<GoogleMapsGeocodingService>>();
        
        var httpClient = new HttpClient();
        _service = new GoogleMapsGeocodingService(httpClient, _loggerMock.Object);
    }

    #region Mock Geocoding Tests

    [Fact]
    public async Task GeocodeAddressAsync_WithNewYorkAddress_ReturnsMockCoordinates()
    {
        // Arrange
        var address = "123 Main St, New York, NY";

        // Act
        var (latitude, longitude) = await _service.GeocodeAddressAsync(address);

        // Assert
        latitude.Should().Be(40.7128);
        longitude.Should().Be(-74.0060);
    }

    [Fact]
    public async Task GeocodeAddressAsync_WithLosAngelesAddress_ReturnsMockCoordinates()
    {
        // Arrange
        var address = "456 Oak Ave, Los Angeles, CA";

        // Act
        var (latitude, longitude) = await _service.GeocodeAddressAsync(address);

        // Assert
        latitude.Should().Be(34.0522);
        longitude.Should().Be(-118.2437);
    }

    [Fact]
    public async Task GeocodeAddressAsync_WithChicagoAddress_ReturnsMockCoordinates()
    {
        // Arrange
        var address = "789 Elm Rd, Chicago, IL";

        // Act
        var (latitude, longitude) = await _service.GeocodeAddressAsync(address);

        // Assert
        latitude.Should().Be(41.8781);
        longitude.Should().Be(-87.6298);
    }

    [Fact]
    public async Task GeocodeAddressAsync_WithDenverAddress_ReturnsMockCoordinates()
    {
        // Arrange
        var address = "100 Pine St, Denver, CO";

        // Act
        var (latitude, longitude) = await _service.GeocodeAddressAsync(address);

        // Assert
        latitude.Should().Be(39.7392);
        longitude.Should().Be(-104.9903);
    }

    [Fact]
    public async Task GeocodeAddressAsync_WithUnknownAddress_ReturnsDefaultCoordinates()
    {
        // Arrange
        var address = "123 Random St, Nowhere, XY 12345";

        // Act
        var (latitude, longitude) = await _service.GeocodeAddressAsync(address);

        // Assert - Should return US center coordinates
        latitude.Should().Be(39.8283);
        longitude.Should().Be(-98.5795);
    }

    [Fact]
    public async Task GeocodeAddressAsync_WithEmptyAddress_ReturnsDefaultCoordinates()
    {
        // Arrange
        var address = "";

        // Act
        var (latitude, longitude) = await _service.GeocodeAddressAsync(address);

        // Assert
        latitude.Should().Be(39.8283);
        longitude.Should().Be(-98.5795);
    }

    [Fact]
    public async Task GeocodeAddressAsync_WithNullAddress_ReturnsDefaultCoordinates()
    {
        // Arrange
        string? address = null;

        // Act
        var (latitude, longitude) = await _service.GeocodeAddressAsync(address!);

        // Assert
        latitude.Should().Be(39.8283);
        longitude.Should().Be(-98.5795);
    }

    [Fact]
    public async Task GeocodeAddressAsync_CachesResults_ReturnsSameCoordinatesOnSecondCall()
    {
        // Arrange
        var address = "123 Main St, Seattle, WA";

        // Act - First call
        var (lat1, lng1) = await _service.GeocodeAddressAsync(address);
        
        // Act - Second call (should be from cache)
        var (lat2, lng2) = await _service.GeocodeAddressAsync(address);

        // Assert
        lat1.Should().Be(lat2);
        lng1.Should().Be(lng2);
        lat1.Should().Be(47.6062);
        lng1.Should().Be(-122.3321);
    }

    [Fact]
    public async Task GeocodeAddressAsync_IgnoresCaseDifferences_ReturnsCachedResults()
    {
        // Arrange
        var address1 = "123 Main St, Seattle, WA";
        var address2 = "123 MAIN ST, SEATTLE, WA";

        // Act - First call with one case
        var (lat1, lng1) = await _service.GeocodeAddressAsync(address1);
        
        // Act - Second call with different case (should use cache)
        var (lat2, lng2) = await _service.GeocodeAddressAsync(address2);

        // Assert - Should return same coordinates
        lat1.Should().Be(lat2);
        lng1.Should().Be(lng2);
    }

    [Fact]
    public async Task GeocodeAddressAsync_MultipleAddresses_ReturnsCorrectCoordinates()
    {
        // Arrange
        var addresses = new[]
        {
            ("123 Main St, Boston, MA", 42.3601, -71.0589),
            ("456 Oak Ave, Miami, FL", 25.7617, -80.1918),
            ("789 Elm Rd, Atlanta, GA", 33.7490, -84.3880)
        };

        // Act & Assert
        foreach (var (address, expectedLat, expectedLng) in addresses)
        {
            var (latitude, longitude) = await _service.GeocodeAddressAsync(address);
            latitude.Should().Be(expectedLat, $"Latitude for {address} should match");
            longitude.Should().Be(expectedLng, $"Longitude for {address} should match");
        }
    }

    #endregion
}


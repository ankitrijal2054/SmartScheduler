using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Infrastructure.Services;
using System.Net;
using Xunit;

namespace SmartScheduler.Infrastructure.Tests.Services;

public class GoogleMapsDistanceServiceTests
{
    private readonly Mock<ILogger<GoogleMapsDistanceService>> _loggerMock;
    private readonly string _apiKey = "test-api-key";

    public GoogleMapsDistanceServiceTests()
    {
        _loggerMock = new Mock<ILogger<GoogleMapsDistanceService>>();
    }

    private HttpClient CreateMockedHttpClient(HttpResponseMessage responseMessage)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        return new HttpClient(handlerMock.Object);
    }

    [Fact]
    public async Task GetDistance_ValidCoordinates_ReturnsDistanceInMiles()
    {
        // Arrange
        var mockResponse = new GoogleMapsDistanceMatrixResponseDto
        {
            Status = "OK",
            Rows = new List<GoogleMapsDistanceMatrixRowDto>
            {
                new()
                {
                    Elements = new List<GoogleMapsDistanceMatrixElementDto>
                    {
                        new()
                        {
                            Status = "OK",
                            Distance = new GoogleMapsValueDto { Value = 16093 }, // ~10 miles in meters
                            Duration = new GoogleMapsValueDto { Value = 1200 }   // 20 minutes
                        }
                    }
                }
            }
        };

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(mockResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(jsonResponse) };

        var httpClient = CreateMockedHttpClient(httpResponse);
        var service = new GoogleMapsDistanceService(httpClient, _apiKey, _loggerMock.Object);

        // Act
        var result = await service.GetDistance(40.7128m, -74.0060m, 41.8781m, -87.6298m);

        // Assert
        result.Should().BeGreaterThan(0);
        result.Should().BeLessThan(20); // Should be around 10 miles
    }

    [Fact]
    public async Task GetTravelTime_ValidCoordinates_ReturnsTravelTimeInMinutes()
    {
        // Arrange
        var mockResponse = new GoogleMapsDistanceMatrixResponseDto
        {
            Status = "OK",
            Rows = new List<GoogleMapsDistanceMatrixRowDto>
            {
                new()
                {
                    Elements = new List<GoogleMapsDistanceMatrixElementDto>
                    {
                        new()
                        {
                            Status = "OK",
                            Distance = new GoogleMapsValueDto { Value = 16093 },
                            Duration = new GoogleMapsValueDto { Value = 1200 }
                        }
                    }
                }
            }
        };

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(mockResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(jsonResponse) };

        var httpClient = CreateMockedHttpClient(httpResponse);
        var service = new GoogleMapsDistanceService(httpClient, _apiKey, _loggerMock.Object);

        // Act
        var result = await service.GetTravelTime(40.7128m, -74.0060m, 41.8781m, -87.6298m);

        // Assert
        result.Should().Be(20); // 1200 seconds = 20 minutes
    }

    [Fact]
    public async Task GetDistanceBatch_MultipleOriginDestinationPairs_ReturnsMatrix()
    {
        // Arrange
        var mockResponse = new GoogleMapsDistanceMatrixResponseDto
        {
            Status = "OK",
            Rows = new List<GoogleMapsDistanceMatrixRowDto>
            {
                new()
                {
                    Elements = new List<GoogleMapsDistanceMatrixElementDto>
                    {
                        new() { Status = "OK", Distance = new GoogleMapsValueDto { Value = 8047 }, Duration = new GoogleMapsValueDto { Value = 600 } },
                        new() { Status = "OK", Distance = new GoogleMapsValueDto { Value = 24140 }, Duration = new GoogleMapsValueDto { Value = 1800 } }
                    }
                },
                new()
                {
                    Elements = new List<GoogleMapsDistanceMatrixElementDto>
                    {
                        new() { Status = "OK", Distance = new GoogleMapsValueDto { Value = 16094 }, Duration = new GoogleMapsValueDto { Value = 1200 } },
                        new() { Status = "OK", Distance = new GoogleMapsValueDto { Value = 32187 }, Duration = new GoogleMapsValueDto { Value = 2400 } }
                    }
                }
            }
        };

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(mockResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(jsonResponse) };

        var httpClient = CreateMockedHttpClient(httpResponse);
        var service = new GoogleMapsDistanceService(httpClient, _apiKey, _loggerMock.Object);

        var origins = new List<(decimal, decimal)> { (40.7128m, -74.0060m), (41.8781m, -87.6298m) };
        var destinations = new List<(decimal, decimal)> { (39.9526m, -75.1652m), (42.3314m, -83.0458m) };

        // Act
        var result = await service.GetDistanceBatch(origins, destinations);

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().HaveCount(2);
        result[1].Should().HaveCount(2);
        result[0][0].Distance.Should().BeGreaterThan(0);
        result[0][0].TravelTime.Should().Be(10);
        result[1][1].TravelTime.Should().Be(40);
    }

    [Theory]
    [InlineData(-91, 0)]    // Latitude out of range
    [InlineData(0, -181)]   // Longitude out of range
    [InlineData(91, 0)]     // Latitude out of range
    [InlineData(0, 181)]    // Longitude out of range
    public async Task GetDistance_InvalidCoordinates_ThrowsArgumentException(decimal lat, decimal lng)
    {
        // Arrange
        var httpClient = new HttpClient();
        var service = new GoogleMapsDistanceService(httpClient, _apiKey, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetDistance(lat, lng, 40m, -74m));
    }

    [Fact]
    public async Task GetDistance_GoogleMapsReturnsNotFound_ReturnsFallbackDistance()
    {
        // Arrange
        var mockResponse = new GoogleMapsDistanceMatrixResponseDto
        {
            Status = "ZERO_RESULTS"
        };

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(mockResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(jsonResponse) };

        var httpClient = CreateMockedHttpClient(httpResponse);
        var service = new GoogleMapsDistanceService(httpClient, _apiKey, _loggerMock.Object);

        // Act
        var result = await service.GetDistance(40.7128m, -74.0060m, 41.8781m, -87.6298m);

        // Assert - Haversine formula should be applied
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetDistance_HttpRequestFails_ReturnsFallbackDistance()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new GoogleMapsDistanceService(httpClient, _apiKey, _loggerMock.Object);

        // Act
        var result = await service.GetDistance(40.7128m, -74.0060m, 41.8781m, -87.6298m);

        // Assert - Should use Haversine fallback
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetDistance_ApiKeyNull_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new GoogleMapsDistanceService(new HttpClient(), null!, _loggerMock.Object));
    }

    [Fact]
    public async Task GetDistanceBatch_EmptyOrigins_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = new HttpClient();
        var service = new GoogleMapsDistanceService(httpClient, _apiKey, _loggerMock.Object);

        var origins = new List<(decimal, decimal)>();
        var destinations = new List<(decimal, decimal)> { (40m, -74m) };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetDistanceBatch(origins, destinations));
    }

    [Fact]
    public async Task GetDistanceBatch_ValidMultipleDestinations_AllResultsPopulated()
    {
        // Arrange
        var mockResponse = new GoogleMapsDistanceMatrixResponseDto
        {
            Status = "OK",
            Rows = new List<GoogleMapsDistanceMatrixRowDto>
            {
                new()
                {
                    Elements = new List<GoogleMapsDistanceMatrixElementDto>
                    {
                        new() { Status = "OK", Distance = new GoogleMapsValueDto { Value = 8047 }, Duration = new GoogleMapsValueDto { Value = 600 } },
                        new() { Status = "OK", Distance = new GoogleMapsValueDto { Value = 16094 }, Duration = new GoogleMapsValueDto { Value = 1200 } },
                        new() { Status = "OK", Distance = new GoogleMapsValueDto { Value = 24141 }, Duration = new GoogleMapsValueDto { Value = 1800 } }
                    }
                }
            }
        };

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(mockResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(jsonResponse) };

        var httpClient = CreateMockedHttpClient(httpResponse);
        var service = new GoogleMapsDistanceService(httpClient, _apiKey, _loggerMock.Object);

        var origins = new List<(decimal, decimal)> { (40.7128m, -74.0060m) };
        var destinations = new List<(decimal, decimal)> { (39.9526m, -75.1652m), (41.8781m, -87.6298m), (42.3314m, -83.0458m) };

        // Act
        var result = await service.GetDistanceBatch(origins, destinations);

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().HaveCount(3);
        result[0][0].Status.Should().Be("OK");
        result[0][1].Status.Should().Be("OK");
        result[0][2].Status.Should().Be("OK");
    }

    [Fact]
    public async Task GetDistance_BoundaryCoordinates_Success()
    {
        // Arrange
        var mockResponse = new GoogleMapsDistanceMatrixResponseDto
        {
            Status = "OK",
            Rows = new List<GoogleMapsDistanceMatrixRowDto>
            {
                new()
                {
                    Elements = new List<GoogleMapsDistanceMatrixElementDto>
                    {
                        new()
                        {
                            Status = "OK",
                            Distance = new GoogleMapsValueDto { Value = 16093 },
                            Duration = new GoogleMapsValueDto { Value = 1200 }
                        }
                    }
                }
            }
        };

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(mockResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(jsonResponse) };

        var httpClient = CreateMockedHttpClient(httpResponse);
        var service = new GoogleMapsDistanceService(httpClient, _apiKey, _loggerMock.Object);

        // Act - Test boundary values
        var result1 = await service.GetDistance(-90, -180, 90, 180);
        var result2 = await service.GetDistance(90, 180, -90, -180);

        // Assert
        result1.Should().BeGreaterThan(0);
        result2.Should().BeGreaterThan(0);
    }
}


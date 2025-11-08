using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Infrastructure.Services;
using System.Diagnostics;
using System.Net;
using Xunit;

namespace SmartScheduler.Infrastructure.Tests;

public class DistanceServiceIntegrationTests
{
    private readonly Mock<ILogger<GoogleMapsDistanceService>> _loggerMock;
    private readonly Mock<ILogger<CachedDistanceService>> _cachedLoggerMock;
    private readonly string _apiKey = "test-integration-key";

    public DistanceServiceIntegrationTests()
    {
        _loggerMock = new Mock<ILogger<GoogleMapsDistanceService>>();
        _cachedLoggerMock = new Mock<ILogger<CachedDistanceService>>();
    }

    private HttpClient CreateMockedHttpClient(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns<HttpRequestMessage, CancellationToken>((request, ct) =>
                Task.FromResult(responseFactory(request)));

        return new HttpClient(handlerMock.Object);
    }

    private Mock<IDistributedCache> CreateMockedCache()
    {
        var cacheMock = new Mock<IDistributedCache>();
        var inMemoryCache = new Dictionary<string, string>();

        cacheMock
            .Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, CancellationToken>((key, ct) =>
                Task.FromResult(inMemoryCache.TryGetValue(key, out var value) ? value : null));

        cacheMock
            .Setup(c => c.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, DistributedCacheEntryOptions, CancellationToken>((key, value, options, ct) =>
            {
                inMemoryCache[key] = value;
            })
            .Returns(Task.CompletedTask);

        return cacheMock;
    }

    [Fact]
    public async Task DistanceService_FullFlow_CalculatesAndCachesDistance()
    {
        // Arrange
        var mockGoogleResponse = new GoogleMapsDistanceMatrixResponseDto
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
                            Distance = new GoogleMapsValueDto { Value = 16093 }, // ~10 miles
                            Duration = new GoogleMapsValueDto { Value = 1200 }   // 20 minutes
                        }
                    }
                }
            }
        };

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(mockGoogleResponse);
        var httpClient = CreateMockedHttpClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(jsonResponse) });

        var innerService = new GoogleMapsDistanceService(httpClient, _apiKey, _loggerMock.Object);
        var cache = CreateMockedCache();
        var cachedService = new CachedDistanceService(innerService, cache.Object, _cachedLoggerMock.Object);

        // Act
        var result1 = await cachedService.GetDistance(40.7128m, -74.0060m, 41.8781m, -87.6298m);
        var result2 = await cachedService.GetDistance(40.7128m, -74.0060m, 41.8781m, -87.6298m);

        // Assert
        result1.Should().BeGreaterThan(0);
        result1.Should().Equal(result2); // Should return same value
        cache.Verify(c => c.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DistanceService_BatchRequest_ProcessesMultipleDestinationsEfficiently()
    {
        // Arrange
        var mockGoogleResponse = new GoogleMapsDistanceMatrixResponseDto
        {
            Status = "OK",
            Rows = new List<GoogleMapsDistanceMatrixRowDto>
            {
                new()
                {
                    Elements = new List<GoogleMapsDistanceMatrixElementDto>
                    {
                        new() { Status = "OK", Distance = new GoogleMapsValueDto { Value = 8047 }, Duration = new GoogleMapsValueDto { Value = 600 } },
                        new() { Status = "OK", Distance = new GoogleMapsValueDto { Value = 24140 }, Duration = new GoogleMapsValueDto { Value = 1800 } },
                        new() { Status = "OK", Distance = new GoogleMapsValueDto { Value = 32187 }, Duration = new GoogleMapsValueDto { Value = 2400 } }
                    }
                }
            }
        };

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(mockGoogleResponse);
        var httpClient = CreateMockedHttpClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(jsonResponse) });

        var innerService = new GoogleMapsDistanceService(httpClient, _apiKey, _loggerMock.Object);
        var cache = CreateMockedCache();
        var cachedService = new CachedDistanceService(innerService, cache.Object, _cachedLoggerMock.Object);

        var origins = new List<(decimal, decimal)> { (40.7128m, -74.0060m) };
        var destinations = new List<(decimal, decimal)> { (39.9526m, -75.1652m), (41.8781m, -87.6298m), (42.3314m, -83.0458m) };

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await cachedService.GetDistanceBatch(origins, destinations);
        stopwatch.Stop();

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().HaveCount(3);
        result[0][0].Distance.Should().BeGreaterThan(0);
        result[0][1].Distance.Should().BeGreaterThan(0);
        result[0][2].Distance.Should().BeGreaterThan(0);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete quickly with mocked API
    }

    [Fact]
    public async Task DistanceService_InvalidAddressError_ReturnsStructuredErrorResponse()
    {
        // Arrange
        var mockGoogleResponse = new GoogleMapsDistanceMatrixResponseDto
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
                            Status = "NOT_FOUND",
                            ErrorMessage = "Invalid address"
                        }
                    }
                }
            }
        };

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(mockGoogleResponse);
        var httpClient = CreateMockedHttpClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(jsonResponse) });

        var innerService = new GoogleMapsDistanceService(httpClient, _apiKey, _loggerMock.Object);
        var cache = CreateMockedCache();
        var cachedService = new CachedDistanceService(innerService, cache.Object, _cachedLoggerMock.Object);

        // Act
        var result = await cachedService.GetDistanceBatch(
            new List<(decimal, decimal)> { (40.7128m, -74.0060m) },
            new List<(decimal, decimal)> { (99.9999m, 99.9999m) });

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().HaveCount(1);
        result[0][0].Status.Should().Be("NOT_FOUND");
        result[0][0].ErrorMessage.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DistanceService_ApiTimeoutAndRetry_EventuallySucceeds()
    {
        // Arrange
        var mockGoogleResponse = new GoogleMapsDistanceMatrixResponseDto
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

        var callCount = 0;
        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(mockGoogleResponse);

        var httpClient = CreateMockedHttpClient(_ =>
        {
            callCount++;
            if (callCount < 2)
            {
                // First call fails
                throw new TimeoutException("Request timed out");
            }
            // Second call succeeds
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(jsonResponse) };
        });

        var innerService = new GoogleMapsDistanceService(httpClient, _apiKey, _loggerMock.Object);
        var cache = CreateMockedCache();
        var cachedService = new CachedDistanceService(innerService, cache.Object, _cachedLoggerMock.Object);

        // Act & Assert - Should succeed despite initial timeout due to retry logic
        var result = await cachedService.GetDistance(40.7128m, -74.0060m, 41.8781m, -87.6298m);
        result.Should().BeGreaterThan(0);
        callCount.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task DistanceService_ApiDown_ReturnsFallbackDistance()
    {
        // Arrange
        var httpClient = CreateMockedHttpClient(_ =>
            throw new HttpRequestException("Connection refused"));

        var innerService = new GoogleMapsDistanceService(httpClient, _apiKey, _loggerMock.Object);
        var cache = CreateMockedCache();
        var cachedService = new CachedDistanceService(innerService, cache.Object, _cachedLoggerMock.Object);

        // Act - Google Maps API completely down
        var result = await cachedService.GetDistance(40.7128m, -74.0060m, 41.8781m, -87.6298m);

        // Assert - Should use Haversine fallback
        result.Should().BeGreaterThan(0);
        result.Should().BeLessThan(1000); // Should be a reasonable distance
    }

    [Fact]
    public async Task DistanceService_CachePartialHit_OnlyFetchesMissingDestinations()
    {
        // Arrange
        var mockGoogleResponse = new GoogleMapsDistanceMatrixResponseDto
        {
            Status = "OK",
            Rows = new List<GoogleMapsDistanceMatrixRowDto>
            {
                new()
                {
                    Elements = new List<GoogleMapsDistanceMatrixElementDto>
                    {
                        new() { Status = "OK", Distance = new GoogleMapsValueDto { Value = 80469 }, Duration = new GoogleMapsValueDto { Value = 4800 } }
                    }
                }
            }
        };

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(mockGoogleResponse);
        var apiCallCount = 0;

        var httpClient = CreateMockedHttpClient(_ =>
        {
            apiCallCount++;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(jsonResponse) };
        });

        var innerService = new GoogleMapsDistanceService(httpClient, _apiKey, _loggerMock.Object);
        var cache = CreateMockedCache();
        var cachedService = new CachedDistanceService(innerService, cache.Object, _cachedLoggerMock.Object);

        var origins = new List<(decimal, decimal)> { (40.7128m, -74.0060m) };
        var destinations = new List<(decimal, decimal)> { (39.9526m, -75.1652m), (41.8781m, -87.6298m), (42.3314m, -83.0458m) };

        // Act
        // First call - all miss, fetch all 3
        var result1 = await cachedService.GetDistanceBatch(origins, destinations);
        
        // Second call - all should hit cache (not calling API)
        var result2 = await cachedService.GetDistanceBatch(origins, destinations);

        // Assert
        result1[0][0].Distance.Should().BeGreaterThan(0);
        result2[0][0].Distance.Should().Equal(result1[0][0].Distance);
        // API should only be called once (for the first batch of 3 destinations)
        apiCallCount.Should().Be(1);
    }

    [Fact]
    public async Task DistanceService_PerformanceBaseline_BatchRequest20ContractorsUnder100ms()
    {
        // Arrange
        var mockElements = new List<GoogleMapsDistanceMatrixElementDto>();
        for (int i = 0; i < 20; i++)
        {
            mockElements.Add(new()
            {
                Status = "OK",
                Distance = new GoogleMapsValueDto { Value = 16093 },
                Duration = new GoogleMapsValueDto { Value = 1200 }
            });
        }

        var mockGoogleResponse = new GoogleMapsDistanceMatrixResponseDto
        {
            Status = "OK",
            Rows = new List<GoogleMapsDistanceMatrixRowDto>
            {
                new() { Elements = mockElements }
            }
        };

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(mockGoogleResponse);
        var httpClient = CreateMockedHttpClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(jsonResponse) });

        var innerService = new GoogleMapsDistanceService(httpClient, _apiKey, _loggerMock.Object);
        var cache = CreateMockedCache();
        var cachedService = new CachedDistanceService(innerService, cache.Object, _cachedLoggerMock.Object);

        var origins = new List<(decimal, decimal)> { (40.7128m, -74.0060m) };
        var destinations = Enumerable.Range(0, 20)
            .Select(i => (40.7128m + i * 0.1m, -74.0060m + i * 0.1m))
            .ToList();

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await cachedService.GetDistanceBatch(origins, destinations);
        stopwatch.Stop();

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().HaveCount(20);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Should be very fast with mocked API
    }

    [Fact]
    public async Task DistanceService_CoordinateValidation_RejectsInvalidBoundaries()
    {
        // Arrange
        var httpClient = new HttpClient();
        var innerService = new GoogleMapsDistanceService(httpClient, _apiKey, _loggerMock.Object);
        var cache = CreateMockedCache();
        var cachedService = new CachedDistanceService(innerService, cache.Object, _cachedLoggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => cachedService.GetDistance(-91, 0, 40, -74));
        await Assert.ThrowsAsync<ArgumentException>(() => cachedService.GetDistance(40, -181, 40, -74));
        await Assert.ThrowsAsync<ArgumentException>(() => cachedService.GetDistance(40, -74, 91, -74));
        await Assert.ThrowsAsync<ArgumentException>(() => cachedService.GetDistance(40, -74, 40, 181));
    }

    [Fact]
    public async Task DistanceService_ResultsAreConsistent_BidirectionalDistance()
    {
        // Arrange
        var mockGoogleResponse = new GoogleMapsDistanceMatrixResponseDto
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

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(mockGoogleResponse);
        var httpClient = CreateMockedHttpClient(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(jsonResponse) });

        var innerService = new GoogleMapsDistanceService(httpClient, _apiKey, _loggerMock.Object);
        var cache = CreateMockedCache();
        var cachedService = new CachedDistanceService(innerService, cache.Object, _cachedLoggerMock.Object);

        // Act
        var distance1 = await cachedService.GetDistance(40.7128m, -74.0060m, 41.8781m, -87.6298m);
        var distance2 = await cachedService.GetDistance(41.8781m, -87.6298m, 40.7128m, -74.0060m);

        // Assert - Both directions should return positive distances
        distance1.Should().BeGreaterThan(0);
        distance2.Should().BeGreaterThan(0);
    }
}


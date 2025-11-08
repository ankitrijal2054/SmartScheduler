using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Services;
using SmartScheduler.Infrastructure.Services;
using Xunit;

namespace SmartScheduler.Infrastructure.Tests.Services;

public class CachedDistanceServiceTests
{
    private readonly Mock<IDistanceService> _innerServiceMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<CachedDistanceService>> _loggerMock;

    public CachedDistanceServiceTests()
    {
        _innerServiceMock = new Mock<IDistanceService>();
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<CachedDistanceService>>();
    }

    [Fact]
    public async Task GetDistance_CacheHit_ReturnsValueWithoutCallingInnerService()
    {
        // Arrange
        const decimal expectedDistance = 25.5m;
        var cacheKey = "distance:40.71280,74.00600:41.87810,87.62980:distance";

        _cacheMock
            .Setup(c => c.GetStringAsync(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDistance.ToString());

        var service = new CachedDistanceService(_innerServiceMock.Object, _cacheMock.Object, _loggerMock.Object);

        // Act
        var result = await service.GetDistance(40.7128m, -74.0060m, 41.8781m, -87.6298m);

        // Assert
        result.Should().Be(expectedDistance);
        _innerServiceMock.Verify(s => s.GetDistance(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task GetDistance_CacheMiss_CallsInnerServiceAndCachesResult()
    {
        // Arrange
        const decimal expectedDistance = 25.5m;
        var cacheKey = "distance:40.71280,74.00600:41.87810,87.62980:distance";

        _cacheMock
            .Setup(c => c.GetStringAsync(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        _innerServiceMock
            .Setup(s => s.GetDistance(40.7128m, -74.0060m, 41.8781m, -87.6298m))
            .ReturnsAsync(expectedDistance);

        _cacheMock
            .Setup(c => c.SetStringAsync(cacheKey, expectedDistance.ToString(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new CachedDistanceService(_innerServiceMock.Object, _cacheMock.Object, _loggerMock.Object);

        // Act
        var result = await service.GetDistance(40.7128m, -74.0060m, 41.8781m, -87.6298m);

        // Assert
        result.Should().Be(expectedDistance);
        _innerServiceMock.Verify(s => s.GetDistance(40.7128m, -74.0060m, 41.8781m, -87.6298m), Times.Once);
        _cacheMock.Verify(c => c.SetStringAsync(cacheKey, expectedDistance.ToString(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTravelTime_CacheHit_ReturnsValueWithoutCallingInnerService()
    {
        // Arrange
        const int expectedTravelTime = 45;
        var cacheKey = "distance:40.71280,74.00600:41.87810,87.62980:traveltime";

        _cacheMock
            .Setup(c => c.GetStringAsync(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTravelTime.ToString());

        var service = new CachedDistanceService(_innerServiceMock.Object, _cacheMock.Object, _loggerMock.Object);

        // Act
        var result = await service.GetTravelTime(40.7128m, -74.0060m, 41.8781m, -87.6298m);

        // Assert
        result.Should().Be(expectedTravelTime);
        _innerServiceMock.Verify(s => s.GetTravelTime(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task GetTravelTime_CacheMiss_CallsInnerServiceAndCachesResult()
    {
        // Arrange
        const int expectedTravelTime = 45;
        var cacheKey = "distance:40.71280,74.00600:41.87810,87.62980:traveltime";

        _cacheMock
            .Setup(c => c.GetStringAsync(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        _innerServiceMock
            .Setup(s => s.GetTravelTime(40.7128m, -74.0060m, 41.8781m, -87.6298m))
            .ReturnsAsync(expectedTravelTime);

        _cacheMock
            .Setup(c => c.SetStringAsync(cacheKey, expectedTravelTime.ToString(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new CachedDistanceService(_innerServiceMock.Object, _cacheMock.Object, _loggerMock.Object);

        // Act
        var result = await service.GetTravelTime(40.7128m, -74.0060m, 41.8781m, -87.6298m);

        // Assert
        result.Should().Be(expectedTravelTime);
        _innerServiceMock.Verify(s => s.GetTravelTime(40.7128m, -74.0060m, 41.8781m, -87.6298m), Times.Once);
        _cacheMock.Verify(c => c.SetStringAsync(cacheKey, expectedTravelTime.ToString(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDistance_CacheThrowsException_ProceedsWithInnerService()
    {
        // Arrange
        const decimal expectedDistance = 25.5m;
        var cacheKey = "distance:40.71280,74.00600:41.87810,87.62980:distance";

        _cacheMock
            .Setup(c => c.GetStringAsync(cacheKey, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis unavailable"));

        _innerServiceMock
            .Setup(s => s.GetDistance(40.7128m, -74.0060m, 41.8781m, -87.6298m))
            .ReturnsAsync(expectedDistance);

        var service = new CachedDistanceService(_innerServiceMock.Object, _cacheMock.Object, _loggerMock.Object);

        // Act
        var result = await service.GetDistance(40.7128m, -74.0060m, 41.8781m, -87.6298m);

        // Assert - Should still work even with cache failure
        result.Should().Be(expectedDistance);
        _innerServiceMock.Verify(s => s.GetDistance(40.7128m, -74.0060m, 41.8781m, -87.6298m), Times.Once);
    }

    [Fact]
    public async Task GetDistance_CacheWriteThrowsException_StillReturnsResult()
    {
        // Arrange
        const decimal expectedDistance = 25.5m;
        var cacheKey = "distance:40.71280,74.00600:41.87810,87.62980:distance";

        _cacheMock
            .Setup(c => c.GetStringAsync(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        _innerServiceMock
            .Setup(s => s.GetDistance(40.7128m, -74.0060m, 41.8781m, -87.6298m))
            .ReturnsAsync(expectedDistance);

        _cacheMock
            .Setup(c => c.SetStringAsync(cacheKey, It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cache write failed"));

        var service = new CachedDistanceService(_innerServiceMock.Object, _cacheMock.Object, _loggerMock.Object);

        // Act & Assert - Should not throw, just return the result
        var result = await service.GetDistance(40.7128m, -74.0060m, 41.8781m, -87.6298m);
        result.Should().Be(expectedDistance);
    }

    [Fact]
    public async Task GetDistanceBatch_MultipleCoordinates_CachesMissingResults()
    {
        // Arrange
        var origins = new List<(decimal, decimal)> { (40.7128m, -74.0060m), (41.8781m, -87.6298m) };
        var destinations = new List<(decimal, decimal)> { (39.9526m, -75.1652m), (42.3314m, -83.0458m) };

        var batchResult = new List<List<DistanceResultDto>>
        {
            new()
            {
                new DistanceResultDto { Distance = 10.5m, TravelTime = 15, Status = "OK" },
                new DistanceResultDto { Distance = 150m, TravelTime = 180, Status = "OK" }
            },
            new()
            {
                new DistanceResultDto { Distance = 120m, TravelTime = 140, Status = "OK" },
                new DistanceResultDto { Distance = 230m, TravelTime = 280, Status = "OK" }
            }
        };

        // Cache returns null for all initially (miss)
        _cacheMock
            .Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        _innerServiceMock
            .Setup(s => s.GetDistanceBatch(It.IsAny<List<(decimal, decimal)>>(), It.IsAny<List<(decimal, decimal)>>()))
            .ReturnsAsync(batchResult);

        _cacheMock
            .Setup(c => c.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new CachedDistanceService(_innerServiceMock.Object, _cacheMock.Object, _loggerMock.Object);

        // Act
        var result = await service.GetDistanceBatch(origins, destinations);

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().HaveCount(2);
        result[1].Should().HaveCount(2);
        result[0][0].Distance.Should().Be(10.5m);
        result[1][1].TravelTime.Should().Be(280);
        
        // Verify cache write was called for all results
        _cacheMock.Verify(
            c => c.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetDistanceBatch_AllCached_DoesNotCallInnerService()
    {
        // Arrange
        var origins = new List<(decimal, decimal)> { (40.7128m, -74.0060m) };
        var destinations = new List<(decimal, decimal)> { (39.9526m, -75.1652m) };

        var cachedResult = new DistanceResultDto { Distance = 50m, TravelTime = 60, Status = "OK" };
        var jsonCachedResult = System.Text.Json.JsonSerializer.Serialize(cachedResult);
        var cacheKey = "distance:40.71280,74.00600:39.95260,75.16520:distance";

        _cacheMock
            .Setup(c => c.GetStringAsync(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(jsonCachedResult);

        var service = new CachedDistanceService(_innerServiceMock.Object, _cacheMock.Object, _loggerMock.Object);

        // Act
        var result = await service.GetDistanceBatch(origins, destinations);

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().HaveCount(1);
        result[0][0].Distance.Should().Be(50m);
        
        // Inner service should never be called when all results are cached
        _innerServiceMock.Verify(s => s.GetDistanceBatch(It.IsAny<List<(decimal, decimal)>>(), It.IsAny<List<(decimal, decimal)>>()), Times.Never);
    }

    [Fact]
    public async Task GetDistanceBatch_PartialCache_OnlyFetchesMissing()
    {
        // Arrange
        var origins = new List<(decimal, decimal)> { (40.7128m, -74.0060m) };
        var destinations = new List<(decimal, decimal)> { (39.9526m, -75.1652m), (42.3314m, -83.0458m) };

        var cachedResult = new DistanceResultDto { Distance = 50m, TravelTime = 60, Status = "OK" };
        var jsonCachedResult = System.Text.Json.JsonSerializer.Serialize(cachedResult);
        var cachedKey = "distance:40.71280,74.00600:39.95260,75.16520:distance";
        var missingKey = "distance:40.71280,74.00600:42.33140,83.04580:distance";

        int callCount = 0;
        _cacheMock
            .Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, CancellationToken>((key, ct) =>
            {
                if (key == cachedKey)
                    return Task.FromResult<string?>(jsonCachedResult);
                return Task.FromResult<string?>(null);
            });

        var batchResult = new List<List<DistanceResultDto>>
        {
            new()
            {
                new DistanceResultDto { Distance = 80m, TravelTime = 90, Status = "OK" }
            }
        };

        _innerServiceMock
            .Setup(s => s.GetDistanceBatch(It.IsAny<List<(decimal, decimal)>>(), It.IsAny<List<(decimal, decimal)>>()))
            .Returns<List<(decimal, decimal)>, List<(decimal, decimal)>>((origs, dests) =>
            {
                callCount++;
                return Task.FromResult(batchResult);
            });

        _cacheMock
            .Setup(c => c.SetStringAsync(missingKey, It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new CachedDistanceService(_innerServiceMock.Object, _cacheMock.Object, _loggerMock.Object);

        // Act
        var result = await service.GetDistanceBatch(origins, destinations);

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().HaveCount(2);
        result[0][0].Distance.Should().Be(50m); // From cache
        result[0][1].Distance.Should().Be(80m); // From inner service
    }

    [Fact]
    public async Task GetDistance_CacheTTL_SetTo24Hours()
    {
        // Arrange
        const decimal expectedDistance = 25.5m;
        var cacheKey = "distance:40.71280,74.00600:41.87810,87.62980:distance";
        DistributedCacheEntryOptions? capturedOptions = null;

        _cacheMock
            .Setup(c => c.GetStringAsync(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        _innerServiceMock
            .Setup(s => s.GetDistance(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
            .ReturnsAsync(expectedDistance);

        _cacheMock
            .Setup(c => c.SetStringAsync(cacheKey, It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, DistributedCacheEntryOptions, CancellationToken>((key, value, options, ct) =>
            {
                capturedOptions = options;
            })
            .Returns(Task.CompletedTask);

        var service = new CachedDistanceService(_innerServiceMock.Object, _cacheMock.Object, _loggerMock.Object);

        // Act
        await service.GetDistance(40.7128m, -74.0060m, 41.8781m, -87.6298m);

        // Assert
        capturedOptions.Should().NotBeNull();
        capturedOptions!.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromHours(24));
    }
}


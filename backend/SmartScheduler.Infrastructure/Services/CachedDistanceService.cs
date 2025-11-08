using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Services;

namespace SmartScheduler.Infrastructure.Services;

/// <summary>
/// Decorator service that wraps IDistanceService with Redis caching.
/// Checks cache before making API calls and stores results with 24-hour TTL.
/// </summary>
public class CachedDistanceService : IDistanceService
{
    private readonly IDistanceService _innerService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedDistanceService> _logger;

    private const int CacheTtlHours = 24;

    public CachedDistanceService(
        IDistanceService innerService,
        IDistributedCache cache,
        ILogger<CachedDistanceService> logger)
    {
        _innerService = innerService ?? throw new ArgumentNullException(nameof(innerService));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<decimal> GetDistance(decimal originLat, decimal originLng, decimal destLat, decimal destLng)
    {
        var cacheKey = BuildCacheKey(originLat, originLng, destLat, destLng, "distance");

        try
        {
            // Try to get from cache
            var cachedValue = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedValue) && decimal.TryParse(cachedValue, out var distance))
            {
                _logger.LogDebug("Cache hit for distance: {CacheKey}", cacheKey);
                return distance;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache retrieval failed for key {CacheKey}, proceeding with API call", cacheKey);
            // If cache is unavailable, continue with API call
        }

        // Call underlying service if cache miss
        var result = await _innerService.GetDistance(originLat, originLng, destLat, destLng);

        // Store in cache (don't fail if cache write fails)
        try
        {
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTtlHours)
            };
            await _cache.SetStringAsync(cacheKey, result.ToString(), cacheOptions);
            _logger.LogDebug("Cached distance result: {CacheKey}", cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write to cache for key {CacheKey}", cacheKey);
            // Don't throw; cache is non-critical
        }

        return result;
    }

    public async Task<int> GetTravelTime(decimal originLat, decimal originLng, decimal destLat, decimal destLng)
    {
        var cacheKey = BuildCacheKey(originLat, originLng, destLat, destLng, "traveltime");

        try
        {
            // Try to get from cache
            var cachedValue = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedValue) && int.TryParse(cachedValue, out var travelTime))
            {
                _logger.LogDebug("Cache hit for travel time: {CacheKey}", cacheKey);
                return travelTime;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache retrieval failed for key {CacheKey}, proceeding with API call", cacheKey);
        }

        // Call underlying service if cache miss
        var result = await _innerService.GetTravelTime(originLat, originLng, destLat, destLng);

        // Store in cache
        try
        {
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTtlHours)
            };
            await _cache.SetStringAsync(cacheKey, result.ToString(), cacheOptions);
            _logger.LogDebug("Cached travel time result: {CacheKey}", cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write to cache for key {CacheKey}", cacheKey);
        }

        return result;
    }

    public async Task<List<List<DistanceResultDto>>> GetDistanceBatch(
        List<(decimal lat, decimal lng)> origins,
        List<(decimal lat, decimal lng)> destinations)
    {
        // For batch operations, we cache individual results rather than the entire batch
        var results = new List<List<DistanceResultDto>>();

        foreach (var (origLat, origLng) in origins)
        {
            var rowResults = new List<DistanceResultDto>();

            foreach (var (destLat, destLng) in destinations)
            {
                var cacheKey = BuildCacheKey(origLat, origLng, destLat, destLng, "distance");

                DistanceResultDto? result = null;

                try
                {
                    // Try to get from cache
                    var cachedValue = await _cache.GetStringAsync(cacheKey);
                    if (!string.IsNullOrEmpty(cachedValue))
                    {
                        result = JsonSerializer.Deserialize<DistanceResultDto>(cachedValue);
                        if (result is not null)
                        {
                            _logger.LogDebug("Cache hit for batch distance: {CacheKey}", cacheKey);
                            rowResults.Add(result);
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Cache retrieval failed for key {CacheKey}", cacheKey);
                }

                // Cache miss - will be calculated by batch call below
                rowResults.Add(new DistanceResultDto { Status = "PENDING" });
            }

            results.Add(rowResults);
        }

        // Check if any results are pending (not cached)
        var pendingOriginIndices = new List<int>();
        var pendingDestinationIndices = new List<int>();

        for (int i = 0; i < results.Count; i++)
        {
            for (int j = 0; j < results[i].Count; j++)
            {
                if (results[i][j].Status == "PENDING")
                {
                    if (!pendingOriginIndices.Contains(i))
                        pendingOriginIndices.Add(i);
                    if (!pendingDestinationIndices.Contains(j))
                        pendingDestinationIndices.Add(j);
                }
            }
        }

        // If there are pending results, call underlying service for just those
        if (pendingOriginIndices.Count > 0 && pendingDestinationIndices.Count > 0)
        {
            var pendingOrigins = pendingOriginIndices.Select(i => origins[i]).ToList();
            var pendingDests = pendingDestinationIndices.Select(j => destinations[j]).ToList();

            var batchResults = await _innerService.GetDistanceBatch(pendingOrigins, pendingDests);

            // Update results and cache
            for (int i = 0; i < pendingOriginIndices.Count; i++)
            {
                for (int j = 0; j < pendingDestinationIndices.Count; j++)
                {
                    var result = batchResults[i][j];
                    var origIdx = pendingOriginIndices[i];
                    var destIdx = pendingDestinationIndices[j];

                    results[origIdx][destIdx] = result;

                    // Cache the result
                    try
                    {
                        var cacheKey = BuildCacheKey(origins[origIdx].lat, origins[origIdx].lng, destinations[destIdx].lat, destinations[destIdx].lng, "distance");
                        var cacheOptions = new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTtlHours)
                        };
                        var serialized = JsonSerializer.Serialize(result);
                        await _cache.SetStringAsync(cacheKey, serialized, cacheOptions);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to cache batch result");
                    }
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Invalidates cache entries for a specific contractor location.
    /// Called when contractor location is updated.
    /// </summary>
    public Task InvalidateContractorCache(decimal latitude, decimal longitude)
    {
        try
        {
            // Note: Redis doesn't support prefix deletion, so we'd need to track keys elsewhere
            // For now, we log the intent and rely on TTL for cleanup
            _logger.LogInformation("Cache invalidation requested for contractor at ({Lat},{Lng}). Relying on 24-hour TTL.", latitude, longitude);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache invalidation failed for contractor at ({Lat},{Lng})", latitude, longitude);
        }
        
        return Task.CompletedTask;
    }

    private string BuildCacheKey(decimal originLat, decimal originLng, decimal destLat, decimal destLng, string type)
    {
        return $"distance:{originLat:F5},{originLng:F5}:{destLat:F5},{destLng:F5}:{type}";
    }
}


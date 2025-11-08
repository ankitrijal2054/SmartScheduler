# Distance Service Setup Guide

## Overview

The Distance Service provides real-time distance and travel time calculations between locations using the Google Maps Distance Matrix API with Redis caching for performance optimization.

## Architecture

The service uses a **decorator pattern** with three layers:

1. **GoogleMapsDistanceService** - Core implementation that calls Google Maps API with retry logic and Haversine fallback
2. **CachedDistanceService** - Wrapper that adds Redis caching layer (24-hour TTL)
3. **IDistanceService** - Common interface used by scoring algorithms

## Setup Instructions

### 1. Google Maps API Key Configuration

#### Development Environment

1. Obtain a Google Maps API key from [Google Cloud Console](https://console.cloud.google.com/)
2. Enable the **Distance Matrix API** for your project
3. Update `appsettings.Development.json`:

```json
{
  "GoogleMaps": {
    "ApiKey": "AIzaSy... your-actual-api-key-here"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

#### Production Environment

In AWS production:

1. Store API key in **AWS Secrets Manager** at path: `SmartScheduler/GoogleMaps/ApiKey`
2. The application automatically loads this on startup via `IConfiguration`
3. See `.NET AWS Secrets Manager Integration` below

### 2. Redis Cache Configuration

#### Local Development (Docker)

```bash
# Start Redis container
docker run --name redis-smartscheduler -p 6379:6379 -d redis:7.1-alpine

# Verify connection
redis-cli ping
# Output: PONG
```

#### Production (AWS ElastiCache)

1. Create an ElastiCache Redis cluster in AWS
2. Store connection string in Secrets Manager at: `SmartScheduler/Redis/ConnectionString`
3. Example: `redis-cluster.abc123.ng.0001.use1.cache.amazonaws.com:6379`

### 3. .NET Application Configuration

The service is registered in `InfrastructureServiceExtensions.cs`:

```csharp
// Program.cs loads these services via:
builder.Services.AddInfrastructureServices(builder.Configuration);
```

**Registration Details:**

- `GoogleMapsDistanceService` - HttpClient registered for API calls
- `CachedDistanceService` - Wrapped with Redis caching (main IDistanceService impl)
- `IDistributedCache` - StackExchangeRedis for cache backend

### 4. AWS Secrets Manager Setup

```bash
# Create secrets for development/staging (local values)
aws secretsmanager create-secret \
  --name SmartScheduler/GoogleMaps/ApiKey \
  --secret-string "AIzaSy..."

aws secretsmanager create-secret \
  --name SmartScheduler/Redis/ConnectionString \
  --secret-string "localhost:6379"

# For production, update with actual values
aws secretsmanager update-secret \
  --secret-id SmartScheduler/GoogleMaps/ApiKey \
  --secret-string "AIzaSy... production-key"
```

## Usage

### Basic Distance Calculation

```csharp
// Inject IDistanceService into your service/controller
public class JobAssignmentService
{
    private readonly IDistanceService _distanceService;

    public JobAssignmentService(IDistanceService distanceService)
    {
        _distanceService = distanceService;
    }

    public async Task<bool> CanAssignContractor(Job job, Contractor contractor)
    {
        // Get distance from job site to contractor location
        var distance = await _distanceService.GetDistance(
            job.Latitude, job.Longitude,
            contractor.Latitude, contractor.Longitude
        );

        // Check if within service area (e.g., 50 miles)
        return distance <= 50;
    }
}
```

### Batch Distance Calculation (Performance Optimized)

```csharp
// Calculate distances for multiple contractors in one API call
var topCandidates = await FindTopContractorCandidates(job);

var origins = new List<(decimal, decimal)>
{
    (job.Latitude, job.Longitude)
};

var destinations = topCandidates
    .Select(c => (c.Latitude, c.Longitude))
    .ToList();

var distanceMatrix = await _distanceService.GetDistanceBatch(origins, destinations);

// distanceMatrix[originIdx][destIdx] = DistanceResultDto
var firstContractorDistance = distanceMatrix[0][0].Distance;
var firstContractorTravelTime = distanceMatrix[0][0].TravelTime;
```

## Error Handling

### Coordinate Validation

Coordinates must be within valid ranges:

- **Latitude**: -90 to 90
- **Longitude**: -180 to 180

Invalid coordinates throw `ArgumentException`:

```csharp
try
{
    var distance = await _distanceService.GetDistance(91, 0, 40, -74); // Throws!
}
catch (ArgumentException ex)
{
    // Handle validation error
}
```

### API Error Responses

When Google Maps returns error statuses:

```csharp
var result = await _distanceService.GetDistanceBatch(origins, destinations);

foreach (var row in result)
{
    foreach (var item in row)
    {
        if (item.Status != "OK")
        {
            // Status values: "ZERO_RESULTS", "NOT_FOUND", "REQUEST_DENIED", "FALLBACK_USED"
            _logger.LogWarning($"Distance calc failed: {item.Status} - {item.ErrorMessage}");
        }
        else
        {
            var distance = item.Distance; // In miles
            var travelTime = item.TravelTime; // In minutes
        }
    }
}
```

### Fallback Behavior

When Google Maps API is unavailable:

1. **GoogleMapsDistanceService** catches the exception
2. Applies **Haversine formula** to calculate "as-the-crow-flies" distance
3. Multiplies by **1.3x** to approximate road distance
4. Returns fallback value to prevent job assignment failures
5. Logs error for monitoring

```csharp
// If Google Maps down, Haversine formula automatically applies
var distance = await _distanceService.GetDistance(40.7128m, -74.0060m, 41.8781m, -87.6298m);
// Result: Haversine-calculated distance (no exception thrown)
```

### Cache Failures

If Redis is unavailable:

1. **CachedDistanceService** catches cache exception
2. Falls through to **GoogleMapsDistanceService** directly
3. System continues with live API calls (no caching)
4. Logs warning for monitoring

Redis failures do NOT block the application.

## Performance Characteristics

### Caching Impact

- **Cold start**: First distance query = API latency (~200-500ms)
- **Cache hit**: Subsequent queries = <1ms (Redis lookup)
- **Hit rate**: ~90% (same location pairs repeat frequently)
- **Cost savings**: ~10x reduction in API calls

### Batch Request Performance

- **Single distance**: 1 API call = 1 result
- **Batch of 20**: 1 API call = 20 results (vs 20 separate calls)
- **Target P95**: <500ms for full recommendations (including distance + availability + scoring)

### Limits

- Google Maps API: 100 requests/second, ~40,000/month free
- Redis: Sub-millisecond per-key latency
- Batch size: Recommended max 25 origins × 25 destinations per request

## Monitoring & Logging

### Key Metrics to Track

1. **API Call Latency**: Monitor 95th percentile for performance baseline
2. **Cache Hit Rate**: Should be >85% after warm-up period
3. **Error Rate**: Fallback distance usage indicates API issues
4. **Failed Requests**: Retry logic should handle transient failures

### Log Entries

**Info Level:**

- Successful distance calculation (cached)

**Warning Level:**

- Cache unavailable
- API retry attempt
- Fallback distance calculation

**Error Level:**

- API permanently down after all retries
- Invalid coordinate inputs
- Critical cache failures

### CloudWatch Queries

```
# Find fallback distance usages
fields @timestamp, @message | filter @message like /FALLBACK_USED/

# Monitor API latency
fields @timestamp, latency | stats avg(latency), pct(latency, 95)

# Cache error analysis
fields @timestamp, @message | filter @message like /Cache retrieval failed/
```

## Testing

### Unit Tests

Run tests in `SmartScheduler.Infrastructure.Tests`:

```bash
dotnet test --filter GoogleMapsDistanceServiceTests
dotnet test --filter CachedDistanceServiceTests
```

Tests mock:

- Google Maps HTTP responses
- Redis cache operations
- Timeout and retry scenarios

### Integration Tests

Run full integration tests:

```bash
dotnet test --filter DistanceServiceIntegrationTests
```

**Important**: Integration tests use mocked HTTP clients; no real API calls are made.

### Manual Testing

With real Redis and API key:

```csharp
// In test/debug mode with real services
var httpClient = new HttpClient();
var service = new GoogleMapsDistanceService(
    httpClient,
    "YOUR-REAL-API-KEY",
    _logger
);

var distance = await service.GetDistance(40.7128m, -74.0060m, 41.8781m, -87.6298m);
Console.WriteLine($"Distance: {distance} miles");
```

## Troubleshooting

### "Google Maps API key is not configured"

**Cause**: `GoogleMaps:ApiKey` missing from configuration  
**Fix**: Add to `appsettings.Development.json` or set environment variable

### "Redis connection string is not configured"

**Cause**: `Redis:ConnectionString` missing  
**Fix**: Start Redis container or set connection string in configuration

### "REQUEST_DENIED" status from API

**Cause**: API key is invalid or doesn't have Distance Matrix API enabled  
**Fix**:

1. Check API key in [Google Cloud Console](https://console.cloud.google.com/)
2. Verify Distance Matrix API is enabled for your project
3. Check billing is active

### High API call volume despite caching

**Cause**: Too many unique location pairs or low hit rate  
**Fix**:

1. Analyze cache hit rate via logs
2. Increase cache TTL if appropriate
3. Batch requests to reduce total calls

### Slow distance calculations (>500ms)

**Cause**: Cache misses, network latency, or API timeout  
**Fix**:

1. Verify Redis connectivity
2. Check Google Maps API quota usage
3. Review API response times in CloudWatch
4. Consider reducing batch size if requests are too large

## Security Notes

⚠️ **Never commit API keys to git**

- Use environment variables in development
- Use AWS Secrets Manager in production
- Rotate API keys regularly
- Implement rate limiting on API endpoints
- Monitor for unusual API usage

## References

- [Google Maps Distance Matrix API](https://developers.google.com/maps/documentation/distance-matrix)
- [Microsoft.Extensions.Caching.StackExchangeRedis](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.stackexchangeredis)
- [Redis Documentation](https://redis.io/documentation)
- AWS Secrets Manager: [Configuration Guide](https://docs.aws.amazon.com/secretsmanager/latest/userguide/)

---

**Last Updated**: November 8, 2025  
**Maintained By**: Development Team

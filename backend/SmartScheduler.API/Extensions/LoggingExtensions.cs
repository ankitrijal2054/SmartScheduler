using Serilog.Context;

namespace SmartScheduler.API.Extensions;

/// <summary>
/// Extension methods for structured logging patterns across the application.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Adds request ID to Serilog log context for correlation across layers.
    /// </summary>
    public static void SetCorrelationId(this ILogger logger, string? correlationId)
    {
        if (!string.IsNullOrEmpty(correlationId))
        {
            LogContext.PushProperty("CorrelationId", correlationId);
        }
    }

    /// <summary>
    /// Logs an operation start with context.
    /// </summary>
    public static void LogOperationStart(
        this ILogger logger,
        string operationName,
        Dictionary<string, object>? context = null)
    {
        if (context != null)
        {
            foreach (var kvp in context)
            {
                LogContext.PushProperty(kvp.Key, kvp.Value);
            }
        }

        logger.LogInformation("Operation started: {OperationName}", operationName);
    }

    /// <summary>
    /// Logs an operation completion with result.
    /// </summary>
    public static void LogOperationEnd(
        this ILogger logger,
        string operationName,
        bool success,
        string? message = null)
    {
        var level = success ? LogLevel.Information : LogLevel.Warning;
        logger.Log(level, "Operation completed: {OperationName}, Success: {Success}, Message: {Message}",
            operationName, success, message ?? "N/A");
    }
}


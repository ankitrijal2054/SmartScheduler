using System.Net;
using System.Text.Json;
using SmartScheduler.Application.Responses;
using SmartScheduler.Domain.Exceptions;

namespace SmartScheduler.API.Middleware;

/// <summary>
/// Global exception handling middleware that catches all unhandled exceptions
/// and returns standardized error responses with appropriate HTTP status codes.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var requestId = context.TraceIdentifier;
        var userId = context.User?.FindFirst("sub")?.Value ?? "Anonymous";
        var timestamp = DateTime.UtcNow;

        // Log all exceptions with context
        _logger.LogError(
            exception,
            "Unhandled exception occurred. RequestId: {RequestId}, UserId: {UserId}, Timestamp: {Timestamp}",
            requestId,
            userId,
            timestamp
        );

        var (statusCode, errorCode, errorMessage) = MapExceptionToResponse(exception);

        response.StatusCode = statusCode;

        var errorResponse = new ApiErrorResponse(errorCode, errorMessage, statusCode);

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(errorResponse, options);

        return response.WriteAsync(json);
    }

    private static (int StatusCode, string ErrorCode, string ErrorMessage) MapExceptionToResponse(Exception exception)
    {
        return exception switch
        {
            ValidationException ex => (
                (int)HttpStatusCode.BadRequest,
                "VALIDATION_ERROR",
                ex.Message
            ),
            UnauthorizedException ex => (
                (int)HttpStatusCode.Unauthorized,
                "UNAUTHORIZED",
                ex.Message
            ),
            ForbiddenException ex => (
                (int)HttpStatusCode.Forbidden,
                "FORBIDDEN",
                ex.Message
            ),
            NotFoundException ex => (
                (int)HttpStatusCode.NotFound,
                "NOT_FOUND",
                ex.Message
            ),
            ConflictException ex => (
                (int)HttpStatusCode.Conflict,
                "CONFLICT",
                ex.Message
            ),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                "INTERNAL_SERVER_ERROR",
                "An unexpected error occurred. Please try again later."
            )
        };
    }
}


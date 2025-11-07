using SmartScheduler.Application.Responses;

namespace SmartScheduler.API.Extensions;

/// <summary>
/// Extension methods for generating standardized API responses in controllers.
/// </summary>
public static class ResponseExtensions
{
    public static ApiResponse<T> Ok<T>(this T? data)
    {
        return new ApiResponse<T>(data, 200);
    }

    public static ApiResponse<T> Created<T>(this T? data)
    {
        return new ApiResponse<T>(data, 201);
    }

    public static ApiResponse<T> NoContent<T>()
    {
        return new ApiResponse<T>(default, 204);
    }

    public static ApiErrorResponse BadRequest(string message = "Bad Request")
    {
        return new ApiErrorResponse("VALIDATION_ERROR", message, 400);
    }

    public static ApiErrorResponse Unauthorized(string message = "Unauthorized")
    {
        return new ApiErrorResponse("UNAUTHORIZED", message, 401);
    }

    public static ApiErrorResponse Forbidden(string message = "Forbidden")
    {
        return new ApiErrorResponse("FORBIDDEN", message, 403);
    }

    public static ApiErrorResponse NotFound(string message = "Resource not found")
    {
        return new ApiErrorResponse("NOT_FOUND", message, 404);
    }

    public static ApiErrorResponse Conflict(string message = "Resource already exists")
    {
        return new ApiErrorResponse("CONFLICT", message, 409);
    }

    public static ApiErrorResponse InternalServerError(string message = "Internal Server Error")
    {
        return new ApiErrorResponse("INTERNAL_SERVER_ERROR", message, 500);
    }
}


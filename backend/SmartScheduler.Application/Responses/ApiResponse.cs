namespace SmartScheduler.Application.Responses;

/// <summary>
/// Generic wrapper for successful API responses.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public int StatusCode { get; set; }

    public ApiResponse(T? data, int statusCode = 200)
    {
        Success = true;
        Data = data;
        StatusCode = statusCode;
    }

    public ApiResponse(int statusCode = 200)
    {
        Success = true;
        Data = default;
        StatusCode = statusCode;
    }
}

/// <summary>
/// Error details within error response.
/// </summary>
public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }

    public ApiError(string code, string message, int statusCode)
    {
        Code = code;
        Message = message;
        StatusCode = statusCode;
    }
}

/// <summary>
/// Wrapper for error API responses.
/// </summary>
public class ApiErrorResponse
{
    public bool Success { get; set; }
    public ApiError Error { get; set; }

    public ApiErrorResponse(string code, string message, int statusCode)
    {
        Success = false;
        Error = new ApiError(code, message, statusCode);
    }
}


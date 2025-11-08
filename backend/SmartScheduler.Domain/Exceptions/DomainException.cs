namespace SmartScheduler.Domain.Exceptions;

/// <summary>
/// Base exception for domain layer - represents business logic violations.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when a requested resource is not found.
/// Maps to HTTP 404 Not Found.
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>
/// Thrown when validation fails (invalid request data).
/// Maps to HTTP 400 Bad Request.
/// </summary>
public class ValidationException : DomainException
{
    public ValidationException(string message) : base(message) { }
}

/// <summary>
/// Thrown when user is not authenticated.
/// Maps to HTTP 401 Unauthorized.
/// </summary>
public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message) : base(message) { }
}

/// <summary>
/// Thrown when user lacks permission to perform an action.
/// Maps to HTTP 403 Forbidden.
/// </summary>
public class ForbiddenException : DomainException
{
    public ForbiddenException(string message) : base(message) { }
}

/// <summary>
/// Thrown when requested resource already exists (e.g., duplicate key).
/// Maps to HTTP 409 Conflict.
/// </summary>
public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }
}

/// <summary>
/// Thrown when a contractor is not found.
/// Maps to HTTP 404 Not Found.
/// </summary>
public class ContractorNotFoundException : NotFoundException
{
    public ContractorNotFoundException(string message) : base(message) { }
}


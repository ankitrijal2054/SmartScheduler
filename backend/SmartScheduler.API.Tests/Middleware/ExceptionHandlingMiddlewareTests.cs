using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SmartScheduler.API.Middleware;
using SmartScheduler.Domain.Exceptions;

namespace SmartScheduler.API.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task Invoke_WithValidationException_ReturnsStatusCode400()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var logger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        var next = new RequestDelegate(innerContext =>
        {
            throw new ValidationException("Invalid input");
        });

        var middleware = new ExceptionHandlingMiddleware(next, logger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Invoke_WithUnauthorizedException_ReturnsStatusCode401()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var logger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        var next = new RequestDelegate(innerContext =>
        {
            throw new UnauthorizedException("Not authenticated");
        });

        var middleware = new ExceptionHandlingMiddleware(next, logger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task Invoke_WithForbiddenException_ReturnsStatusCode403()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var logger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        var next = new RequestDelegate(innerContext =>
        {
            throw new ForbiddenException("Access denied");
        });

        var middleware = new ExceptionHandlingMiddleware(next, logger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task Invoke_WithNotFoundException_ReturnsStatusCode404()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var logger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        var next = new RequestDelegate(innerContext =>
        {
            throw new NotFoundException("Resource not found");
        });

        var middleware = new ExceptionHandlingMiddleware(next, logger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Invoke_WithConflictException_ReturnsStatusCode409()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var logger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        var next = new RequestDelegate(innerContext =>
        {
            throw new ConflictException("Resource already exists");
        });

        var middleware = new ExceptionHandlingMiddleware(next, logger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Invoke_WithGenericException_ReturnsStatusCode500()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var logger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        var next = new RequestDelegate(innerContext =>
        {
            throw new Exception("Unexpected error");
        });

        var middleware = new ExceptionHandlingMiddleware(next, logger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task Invoke_WithException_SetsJsonContentType()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var logger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        var next = new RequestDelegate(innerContext =>
        {
            throw new Exception("Test");
        });

        var middleware = new ExceptionHandlingMiddleware(next, logger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.ContentType.Should().Be("application/json");
    }
}


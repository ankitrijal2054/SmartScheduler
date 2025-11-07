using FluentAssertions;
using SmartScheduler.Application.Responses;

namespace SmartScheduler.API.Tests.Responses;

public class ApiResponseTests
{
    [Fact]
    public void ApiResponse_WithData_ShouldHaveSuccessTrue()
    {
        // Arrange & Act
        var response = new ApiResponse<string>("test data", 200);

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().Be("test data");
        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public void ApiResponse_WithoutData_ShouldHaveDefaultData()
    {
        // Arrange & Act
        var response = new ApiResponse<string>(200);

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().BeNull();
        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public void ApiResponse_CreatedStatus_ShouldHave201StatusCode()
    {
        // Arrange & Act
        var response = new ApiResponse<string>("created", 201);

        // Assert
        response.StatusCode.Should().Be(201);
    }

    [Fact]
    public void ApiErrorResponse_ShouldHaveSuccessFalse()
    {
        // Arrange & Act
        var response = new ApiErrorResponse("VALIDATION_ERROR", "Invalid input", 400);

        // Assert
        response.Success.Should().BeFalse();
        response.Error.Code.Should().Be("VALIDATION_ERROR");
        response.Error.Message.Should().Be("Invalid input");
        response.Error.StatusCode.Should().Be(400);
    }

    [Fact]
    public void ApiError_ShouldHaveAllProperties()
    {
        // Arrange & Act
        var error = new ApiError("NOT_FOUND", "Resource not found", 404);

        // Assert
        error.Code.Should().Be("NOT_FOUND");
        error.Message.Should().Be("Resource not found");
        error.StatusCode.Should().Be(404);
    }
}


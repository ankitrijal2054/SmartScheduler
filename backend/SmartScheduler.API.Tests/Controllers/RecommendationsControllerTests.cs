using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SmartScheduler.API.Controllers;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Queries;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Exceptions;

namespace SmartScheduler.API.Tests.Controllers;

public class RecommendationsControllerTests
{
    private readonly RecommendationsController _controller;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<ILogger<RecommendationsController>> _loggerMock;

    public RecommendationsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _loggerMock = new Mock<ILogger<RecommendationsController>>();

        _controller = new RecommendationsController(
            _mediatorMock.Object,
            _loggerMock.Object,
            _authorizationServiceMock.Object
        );
    }

    private void SetupControllerUser(string role, int userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, $"user{userId}@test.com"),
            new Claim(ClaimTypes.Role, role)
        };

        var claimsIdentity = new ClaimsIdentity(claims, "test");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = mockHttpContext.Object
        };
    }

    #region GetRecommendations Tests

    [Fact]
    public async Task GetRecommendations_WithValidJobId_ReturnsOkWithRecommendations()
    {
        // Arrange
        SetupControllerUser("Dispatcher", 1);

        int jobId = 1;
        var mockResponse = new RecommendationResponseDto
        {
            Recommendations = new List<RecommendationDto>
            {
                new RecommendationDto
                {
                    ContractorId = 1,
                    Name = "John Smith",
                    Score = 0.92m,
                    Rating = 4.8m,
                    ReviewCount = 24,
                    Distance = 3.5m,
                    TravelTime = 12,
                    AvailableTimeSlots = new List<DateTime>()
                }
            },
            Message = "Success"
        };

        _authorizationServiceMock
            .Setup(a => a.GetCurrentUserIdFromContext(It.IsAny<ClaimsPrincipal>()))
            .Returns(1);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetContractorRecommendationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetRecommendations(jobId, false);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var returnValue = okResult.Value as RecommendationResponseDto;
        returnValue.Should().NotBeNull();
        returnValue!.Recommendations.Should().HaveCount(1);
        returnValue.Message.Should().Be("Success");
    }

    [Fact]
    public async Task GetRecommendations_WithContractorListOnly_PassesFilterToQuery()
    {
        // Arrange
        SetupControllerUser("Dispatcher", 1);

        int jobId = 1;
        var mockResponse = new RecommendationResponseDto
        {
            Recommendations = new List<RecommendationDto>(),
            Message = "Success"
        };

        _authorizationServiceMock
            .Setup(a => a.GetCurrentUserIdFromContext(It.IsAny<ClaimsPrincipal>()))
            .Returns(1);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetContractorRecommendationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetRecommendations(jobId, contractorListOnly: true);

        // Assert
        _mediatorMock.Verify(m =>
            m.Send(
                It.Is<GetContractorRecommendationsQuery>(q =>
                    q.JobId == jobId &&
                    q.ContractorListOnly == true),
                It.IsAny<CancellationToken>()),
            Times.Once);

        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRecommendations_WithNoContractors_ReturnsOkWithEmptyList()
    {
        // Arrange
        SetupControllerUser("Dispatcher", 1);

        int jobId = 1;
        var mockResponse = new RecommendationResponseDto
        {
            Recommendations = new List<RecommendationDto>(),
            Message = "No available contractors"
        };

        _authorizationServiceMock
            .Setup(a => a.GetCurrentUserIdFromContext(It.IsAny<ClaimsPrincipal>()))
            .Returns(1);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetContractorRecommendationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetRecommendations(jobId, false);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var returnValue = okResult.Value as RecommendationResponseDto;
        returnValue.Should().NotBeNull();
        returnValue!.Recommendations.Should().BeEmpty();
        returnValue.Message.Should().Be("No available contractors");
    }

    [Fact]
    public async Task GetRecommendations_WithInvalidJobId_ReturnsBadRequest()
    {
        // Arrange
        SetupControllerUser("Dispatcher", 1);

        int jobId = 999;

        _authorizationServiceMock
            .Setup(a => a.GetCurrentUserIdFromContext(It.IsAny<ClaimsPrincipal>()))
            .Returns(1);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetContractorRecommendationsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Job with ID {jobId} not found"));

        // Act
        var result = await _controller.GetRecommendations(jobId, false);

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetRecommendations_WithPastDesiredDateTime_ReturnsBadRequest()
    {
        // Arrange
        SetupControllerUser("Dispatcher", 1);

        int jobId = 1;

        _authorizationServiceMock
            .Setup(a => a.GetCurrentUserIdFromContext(It.IsAny<ClaimsPrincipal>()))
            .Returns(1);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetContractorRecommendationsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Desired date/time cannot be in the past"));

        // Act
        var result = await _controller.GetRecommendations(jobId, false);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetRecommendations_WithUnexpectedError_ReturnsInternalServerError()
    {
        // Arrange
        SetupControllerUser("Dispatcher", 1);

        int jobId = 1;

        _authorizationServiceMock
            .Setup(a => a.GetCurrentUserIdFromContext(It.IsAny<ClaimsPrincipal>()))
            .Returns(1);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetContractorRecommendationsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.GetRecommendations(jobId, false);

        // Assert
        var serverResult = result.Result as ObjectResult;
        serverResult.Should().NotBeNull();
        serverResult!.StatusCode.Should().Be(500);
    }

    #endregion
}

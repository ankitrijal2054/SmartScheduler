using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SmartScheduler.API.Controllers;
using SmartScheduler.Application.Commands;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Queries;
using SmartScheduler.Application.Services;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Domain.Exceptions;

namespace SmartScheduler.API.Tests.Controllers;

/// <summary>
/// Unit tests for DispatcherController endpoints.
/// </summary>
public class DispatcherControllerTests
{
    private readonly DispatcherController _controller;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<ILogger<DispatcherController>> _loggerMock;
    private readonly Mock<IContractorService> _contractorServiceMock;

    public DispatcherControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _loggerMock = new Mock<ILogger<DispatcherController>>();
        _contractorServiceMock = new Mock<IContractorService>();

        _controller = new DispatcherController(
            _mediatorMock.Object,
            _loggerMock.Object,
            _authorizationServiceMock.Object,
            _contractorServiceMock.Object
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

    #region PostContractorToList Tests

    [Fact]
    public async Task PostContractorToList_WithValidAuth_Returns200()
    {
        // Arrange
        SetupControllerUser("Dispatcher", 1);

        int contractorId = 10;
        int dispatcherContractorListId = 100;

        _authorizationServiceMock
            .Setup(a => a.GetCurrentUserIdFromContext(It.IsAny<ClaimsPrincipal>()))
            .Returns(1);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<AddContractorToListCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dispatcherContractorListId);

        // Act
        var result = await _controller.PostContractorToList(contractorId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var responseValue = okResult.Value as dynamic;
        responseValue.Should().NotBeNull();
        responseValue.message.Should().Be("Contractor added to your list");
        responseValue.dispatcherContractorListId.Should().Be(dispatcherContractorListId);
        responseValue.contractorId.Should().Be(contractorId);
    }

    [Fact]
    public async Task PostContractorToList_WithInvalidContractorId_ReturnsBadRequest()
    {
        // Arrange
        SetupControllerUser("Dispatcher", 1);

        int contractorId = 999; // Non-existent

        _authorizationServiceMock
            .Setup(a => a.GetCurrentUserIdFromContext(It.IsAny<ClaimsPrincipal>()))
            .Returns(1);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<AddContractorToListCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Contractor with ID {contractorId} not found"));

        // Act
        var result = await _controller.PostContractorToList(contractorId);

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task PostContractorToList_WithDuplicateContractor_ReturnsIdempotent200()
    {
        // Arrange
        SetupControllerUser("Dispatcher", 1);

        int contractorId = 10;
        int dispatcherContractorListId = 100;

        _authorizationServiceMock
            .Setup(a => a.GetCurrentUserIdFromContext(It.IsAny<ClaimsPrincipal>()))
            .Returns(1);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<AddContractorToListCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dispatcherContractorListId);

        // Act - Add twice
        var result1 = await _controller.PostContractorToList(contractorId);
        var result2 = await _controller.PostContractorToList(contractorId);

        // Assert - Both return 200
        (result1.Result as OkObjectResult)!.StatusCode.Should().Be(200);
        (result2.Result as OkObjectResult)!.StatusCode.Should().Be(200);
    }

    #endregion

    #region DeleteContractorFromList Tests

    [Fact]
    public async Task DeleteContractorFromList_WithValidAuth_Returns200()
    {
        // Arrange
        SetupControllerUser("Dispatcher", 1);

        int contractorId = 10;

        _authorizationServiceMock
            .Setup(a => a.GetCurrentUserIdFromContext(It.IsAny<ClaimsPrincipal>()))
            .Returns(1);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RemoveContractorFromListCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _controller.DeleteContractorFromList(contractorId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var responseValue = okResult.Value as dynamic;
        responseValue.Should().NotBeNull();
        responseValue.message.Should().Be("Contractor removed from your list");
        responseValue.contractorId.Should().Be(contractorId);
    }

    [Fact]
    public async Task DeleteContractorFromList_WithNonExistentContractor_ReturnsIdempotent200()
    {
        // Arrange
        SetupControllerUser("Dispatcher", 1);

        int contractorId = 999; // Not in list

        _authorizationServiceMock
            .Setup(a => a.GetCurrentUserIdFromContext(It.IsAny<ClaimsPrincipal>()))
            .Returns(1);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RemoveContractorFromListCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value); // Idempotent - no error

        // Act
        var result = await _controller.DeleteContractorFromList(contractorId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200); // Still 200 due to idempotency
    }

    #endregion

    #region GetContractorList Tests

    [Fact]
    public async Task GetContractorList_WithValidAuth_ReturnsContractorList()
    {
        // Arrange
        SetupControllerUser("Dispatcher", 1);

        var mockResponse = new DispatcherContractorListResponseDto
        {
            Contractors = new List<ContractorListItemDto>
            {
                new ContractorListItemDto
                {
                    Id = 1,
                    Name = "John Plumber",
                    PhoneNumber = "555-1234",
                    Location = "Denver, CO",
                    TradeType = "Plumbing",
                    AverageRating = 4.8m,
                    ReviewCount = 24,
                    TotalJobsCompleted = 42,
                    IsActive = true,
                    AddedAt = DateTime.UtcNow.AddDays(-1)
                }
            },
            Pagination = new PaginationMetadataDto
            {
                Page = 1,
                Limit = 50,
                Total = 1,
                TotalPages = 1
            }
        };

        _authorizationServiceMock
            .Setup(a => a.GetCurrentUserIdFromContext(It.IsAny<ClaimsPrincipal>()))
            .Returns(1);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetDispatcherContractorListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetContractorList(page: 1, limit: 50, search: null);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var responseValue = okResult.Value as dynamic;
        responseValue.Should().NotBeNull();
        responseValue.contractors.Should().HaveCount(1);
        responseValue.pagination.total.Should().Be(1);
    }

    [Fact]
    public async Task GetContractorList_WithPaginationParams_ReturnsPagedResults()
    {
        // Arrange
        SetupControllerUser("Dispatcher", 1);

        var mockResponse = new DispatcherContractorListResponseDto
        {
            Contractors = new List<ContractorListItemDto>
            {
                new ContractorListItemDto
                {
                    Id = 2,
                    Name = "Jane Electrician",
                    PhoneNumber = "555-5678",
                    Location = "Boulder, CO",
                    TradeType = "Electrical",
                    AverageRating = 4.5m,
                    ReviewCount = 18,
                    TotalJobsCompleted = 35,
                    IsActive = true,
                    AddedAt = DateTime.UtcNow
                }
            },
            Pagination = new PaginationMetadataDto
            {
                Page = 2,
                Limit = 1,
                Total = 3,
                TotalPages = 3
            }
        };

        _authorizationServiceMock
            .Setup(a => a.GetCurrentUserIdFromContext(It.IsAny<ClaimsPrincipal>()))
            .Returns(1);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetDispatcherContractorListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetContractorList(page: 2, limit: 1, search: null);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var responseValue = okResult.Value as dynamic;
        responseValue.pagination.page.Should().Be(2);
        responseValue.pagination.limit.Should().Be(1);
        responseValue.pagination.totalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetContractorList_WithSearchFilter_ReturnsFilteredResults()
    {
        // Arrange
        SetupControllerUser("Dispatcher", 1);

        var mockResponse = new DispatcherContractorListResponseDto
        {
            Contractors = new List<ContractorListItemDto>
            {
                new ContractorListItemDto
                {
                    Id = 1,
                    Name = "John Plumber",
                    PhoneNumber = "555-1234",
                    Location = "Denver, CO",
                    TradeType = "Plumbing",
                    AverageRating = 4.8m,
                    ReviewCount = 24,
                    TotalJobsCompleted = 42,
                    IsActive = true,
                    AddedAt = DateTime.UtcNow
                }
            },
            Pagination = new PaginationMetadataDto
            {
                Page = 1,
                Limit = 50,
                Total = 1,
                TotalPages = 1
            }
        };

        _authorizationServiceMock
            .Setup(a => a.GetCurrentUserIdFromContext(It.IsAny<ClaimsPrincipal>()))
            .Returns(1);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetDispatcherContractorListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetContractorList(page: 1, limit: 50, search: "plumber");

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var responseValue = okResult.Value as dynamic;
        responseValue.contractors.Should().HaveCount(1);
        responseValue.contractors[0].name.Should().Be("John Plumber");
    }

    [Fact]
    public async Task GetContractorList_WithEmptyList_ReturnsEmptyContractorsList()
    {
        // Arrange
        SetupControllerUser("Dispatcher", 1);

        var mockResponse = new DispatcherContractorListResponseDto
        {
            Contractors = new List<ContractorListItemDto>(),
            Pagination = new PaginationMetadataDto
            {
                Page = 1,
                Limit = 50,
                Total = 0,
                TotalPages = 0
            }
        };

        _authorizationServiceMock
            .Setup(a => a.GetCurrentUserIdFromContext(It.IsAny<ClaimsPrincipal>()))
            .Returns(1);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetDispatcherContractorListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.GetContractorList(page: 1, limit: 50, search: null);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();

        var responseValue = okResult.Value as dynamic;
        responseValue.contractors.Should().BeEmpty();
        responseValue.pagination.total.Should().Be(0);
    }

    #endregion
}


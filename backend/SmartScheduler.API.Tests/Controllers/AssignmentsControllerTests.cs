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
using SmartScheduler.Application.Repositories;
using SmartScheduler.Application.Responses;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Domain.Exceptions;

namespace SmartScheduler.API.Tests.Controllers;

/// <summary>
/// Unit tests for AssignmentsController endpoints.
/// </summary>
public class AssignmentsControllerTests
{
    private readonly AssignmentsController _controller;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IAssignmentRepository> _repositoryMock;
    private readonly Mock<ILogger<AssignmentsController>> _loggerMock;

    public AssignmentsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _repositoryMock = new Mock<IAssignmentRepository>();
        _loggerMock = new Mock<ILogger<AssignmentsController>>();

        _controller = new AssignmentsController(
            _mediatorMock.Object,
            _repositoryMock.Object,
            _loggerMock.Object
        );
    }

    private void SetupControllerUser(int contractorId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, contractorId.ToString()),
            new Claim(ClaimTypes.Role, "Contractor")
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

    private AssignmentDto CreateAssignmentDto(int id = 1, int jobId = 10, int contractorId = 100)
    {
        return new AssignmentDto
        {
            Id = id,
            JobId = jobId,
            ContractorId = contractorId,
            Status = "InProgress",
            AssignedAt = DateTime.UtcNow.AddDays(-1),
            AcceptedAt = DateTime.UtcNow.AddHours(-2),
            StartedAt = DateTime.UtcNow.AddHours(-1),
            CompletedAt = null
        };
    }

    [Fact]
    public async Task MarkInProgress_WithValidAssignment_Returns200OK()
    {
        // Arrange
        var contractorId = 100;
        var assignmentId = 1;
        SetupControllerUser(contractorId);

        var assignmentDto = CreateAssignmentDto(assignmentId, contractorId: contractorId);
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdateAssignmentStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignmentDto);

        // Act
        var result = await _controller.MarkInProgress(assignmentId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var response = okResult.Value as ApiResponse<AssignmentDto>;
        response.Should().NotBeNull();
        response!.Data.Should().Be(assignmentDto);
    }

    [Fact]
    public async Task MarkComplete_WithValidAssignment_Returns200OK()
    {
        // Arrange
        var contractorId = 100;
        var assignmentId = 1;
        SetupControllerUser(contractorId);

        var assignmentDto = CreateAssignmentDto(assignmentId, contractorId: contractorId);
        assignmentDto.Status = "Completed";
        assignmentDto.CompletedAt = DateTime.UtcNow;

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdateAssignmentStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignmentDto);

        // Act
        var result = await _controller.MarkComplete(assignmentId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task MarkInProgress_AssignmentNotFound_Returns404NotFound()
    {
        // Arrange
        var contractorId = 100;
        var assignmentId = 999;
        SetupControllerUser(contractorId);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdateAssignmentStatusCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AssignmentNotFoundException("Assignment not found"));

        // Act
        var result = await _controller.MarkInProgress(assignmentId);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task MarkInProgress_UnauthorizedContractor_ReturnsForbid()
    {
        // Arrange
        var contractorId = 100;
        var assignmentId = 1;
        SetupControllerUser(contractorId);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdateAssignmentStatusCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

        // Act
        var result = await _controller.MarkInProgress(assignmentId);

        // Assert
        result.Should().NotBeNull();
        var forbidResult = result.Result as ForbidResult;
        forbidResult.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkInProgress_InvalidStatusTransition_Returns400BadRequest()
    {
        // Arrange
        var contractorId = 100;
        var assignmentId = 1;
        SetupControllerUser(contractorId);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdateAssignmentStatusCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cannot mark assignment as in-progress. Current status is InProgress."));

        // Act
        var result = await _controller.MarkInProgress(assignmentId);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetContractorHistory_WithValidContractor_Returns200OK()
    {
        // Arrange
        var contractorId = 100;
        SetupControllerUser(contractorId);

        var assignments = new List<Assignment>
        {
            new Assignment
            {
                Id = 1,
                JobId = 10,
                ContractorId = contractorId,
                Status = AssignmentStatus.Completed,
                AssignedAt = DateTime.UtcNow.AddDays(-5),
                AcceptedAt = DateTime.UtcNow.AddDays(-5),
                StartedAt = DateTime.UtcNow.AddDays(-1),
                CompletedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        _repositoryMock
            .Setup(x => x.GetAssignmentsByContractorAndStatusAsync(contractorId, AssignmentStatus.Completed, 50, 0))
            .ReturnsAsync(assignments);

        _repositoryMock
            .Setup(x => x.GetAssignmentCountByContractorAndStatusAsync(contractorId, AssignmentStatus.Completed))
            .ReturnsAsync(1);

        // Act
        var result = await _controller.GetContractorHistory(contractorId, "Completed", 50, 0);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var response = okResult.Value as ApiResponse<PaginatedResponse<AssignmentDto>>;
        response.Should().NotBeNull();
        response!.Data.Items.Should().HaveCount(1);
        response.Data.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetContractorHistory_UnauthorizedContractor_ReturnsForbid()
    {
        // Arrange
        var authenticatedContractorId = 100;
        var requestedContractorId = 200; // Different contractor
        SetupControllerUser(authenticatedContractorId);

        // Act
        var result = await _controller.GetContractorHistory(requestedContractorId);

        // Assert
        result.Should().NotBeNull();
        var forbidResult = result.Result as ForbidResult;
        forbidResult.Should().NotBeNull();
    }

    [Fact]
    public async Task GetContractorHistory_WithPagination_ReturnsPaginatedResults()
    {
        // Arrange
        var contractorId = 100;
        SetupControllerUser(contractorId);

        var assignments = Enumerable.Range(1, 10)
            .Select(i => new Assignment
            {
                Id = i,
                JobId = 100 + i,
                ContractorId = contractorId,
                Status = AssignmentStatus.Completed,
                AssignedAt = DateTime.UtcNow.AddDays(-i),
                CompletedAt = DateTime.UtcNow.AddDays(-i + 1)
            })
            .ToList();

        _repositoryMock
            .Setup(x => x.GetAssignmentsByContractorAndStatusAsync(contractorId, AssignmentStatus.Completed, 5, 0))
            .ReturnsAsync(assignments.Take(5).ToList());

        _repositoryMock
            .Setup(x => x.GetAssignmentCountByContractorAndStatusAsync(contractorId, AssignmentStatus.Completed))
            .ReturnsAsync(10);

        // Act
        var result = await _controller.GetContractorHistory(contractorId, "Completed", 5, 0);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = okResult?.Value as ApiResponse<PaginatedResponse<AssignmentDto>>;
        response!.Data.Items.Should().HaveCount(5);
        response.Data.TotalCount.Should().Be(10);
        response.Data.PageSize.Should().Be(5);
        response.Data.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task MarkComplete_CallsHandlerWithCorrectCommand()
    {
        // Arrange
        var contractorId = 100;
        var assignmentId = 1;
        SetupControllerUser(contractorId);

        var assignmentDto = CreateAssignmentDto(assignmentId, contractorId: contractorId);
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdateAssignmentStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignmentDto);

        // Act
        await _controller.MarkComplete(assignmentId);

        // Assert
        _mediatorMock.Verify(
            x => x.Send(
                It.Is<UpdateAssignmentStatusCommand>(cmd =>
                    cmd.AssignmentId == assignmentId &&
                    cmd.NewStatus == AssignmentStatus.Completed &&
                    cmd.ContractorId == contractorId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MarkInProgress_CallsHandlerWithCorrectCommand()
    {
        // Arrange
        var contractorId = 100;
        var assignmentId = 1;
        SetupControllerUser(contractorId);

        var assignmentDto = CreateAssignmentDto(assignmentId, contractorId: contractorId);
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdateAssignmentStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignmentDto);

        // Act
        await _controller.MarkInProgress(assignmentId);

        // Assert
        _mediatorMock.Verify(
            x => x.Send(
                It.Is<UpdateAssignmentStatusCommand>(cmd =>
                    cmd.AssignmentId == assignmentId &&
                    cmd.NewStatus == AssignmentStatus.InProgress &&
                    cmd.ContractorId == contractorId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}


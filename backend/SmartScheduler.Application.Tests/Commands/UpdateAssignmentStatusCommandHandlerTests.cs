using FluentAssertions;
using MediatR;
using Moq;
using SmartScheduler.Application.Commands;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Domain.Events;
using SmartScheduler.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace SmartScheduler.Application.Tests.Commands;

public class UpdateAssignmentStatusCommandHandlerTests
{
    private readonly Mock<IAssignmentRepository> _mockAssignmentRepository;
    private readonly Mock<IPublisher> _mockEventPublisher;
    private readonly Mock<ILogger<UpdateAssignmentStatusCommandHandler>> _mockLogger;
    private readonly UpdateAssignmentStatusCommandHandler _handler;

    public UpdateAssignmentStatusCommandHandlerTests()
    {
        _mockAssignmentRepository = new Mock<IAssignmentRepository>();
        _mockEventPublisher = new Mock<IPublisher>();
        _mockLogger = new Mock<ILogger<UpdateAssignmentStatusCommandHandler>>();

        _handler = new UpdateAssignmentStatusCommandHandler(
            _mockAssignmentRepository.Object,
            _mockEventPublisher.Object,
            _mockLogger.Object);
    }

    private Assignment CreateAssignment(int id = 1, int jobId = 10, int contractorId = 100, AssignmentStatus status = AssignmentStatus.Accepted)
    {
        return new Assignment
        {
            Id = id,
            JobId = jobId,
            ContractorId = contractorId,
            Status = status,
            AssignedAt = DateTime.UtcNow.AddDays(-1),
            AcceptedAt = DateTime.UtcNow.AddHours(-2),
            StartedAt = null,
            CompletedAt = null,
            Job = new Job
            {
                Id = jobId,
                CustomerId = 200,
                DesiredDateTime = DateTime.UtcNow.AddDays(1),
                Status = JobStatus.InProgress
            }
        };
    }

    [Fact]
    public async Task MarkInProgress_WithValidAssignment_UpdatesStatusAndPublishesEvent()
    {
        // Arrange
        var assignment = CreateAssignment(status: AssignmentStatus.Accepted);
        var command = new UpdateAssignmentStatusCommand(assignment.Id, AssignmentStatus.InProgress, assignment.ContractorId);

        _mockAssignmentRepository
            .Setup(x => x.GetByIdAsync(command.AssignmentId))
            .ReturnsAsync(assignment);

        _mockAssignmentRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Assignment>()))
            .ReturnsAsync(assignment);

        _mockEventPublisher
            .Setup(x => x.Publish(It.IsAny<JobInProgressEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("InProgress");
        result.StartedAt.Should().NotBeNull();
        _mockAssignmentRepository.Verify(x => x.UpdateAsync(It.IsAny<Assignment>()), Times.Once);
        _mockEventPublisher.Verify(x => x.Publish(It.IsAny<JobInProgressEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkComplete_WithValidAssignment_UpdatesStatusAndPublishesEvent()
    {
        // Arrange
        var assignment = CreateAssignment(status: AssignmentStatus.InProgress);
        assignment.StartedAt = DateTime.UtcNow.AddHours(-1);

        var command = new UpdateAssignmentStatusCommand(assignment.Id, AssignmentStatus.Completed, assignment.ContractorId);

        _mockAssignmentRepository
            .Setup(x => x.GetByIdAsync(command.AssignmentId))
            .ReturnsAsync(assignment);

        _mockAssignmentRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Assignment>()))
            .ReturnsAsync(assignment);

        _mockEventPublisher
            .Setup(x => x.Publish(It.IsAny<JobCompletedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Completed");
        result.CompletedAt.Should().NotBeNull();
        _mockAssignmentRepository.Verify(x => x.UpdateAsync(It.IsAny<Assignment>()), Times.Once);
        _mockEventPublisher.Verify(x => x.Publish(It.IsAny<JobCompletedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkInProgress_AssignmentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var command = new UpdateAssignmentStatusCommand(999, AssignmentStatus.InProgress, 100);

        _mockAssignmentRepository
            .Setup(x => x.GetByIdAsync(command.AssignmentId))
            .ReturnsAsync((Assignment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<AssignmentNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task MarkInProgress_UnauthorizedContractor_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var assignment = CreateAssignment(contractorId: 100);
        var command = new UpdateAssignmentStatusCommand(assignment.Id, AssignmentStatus.InProgress, contractorId: 999); // Different contractor

        _mockAssignmentRepository
            .Setup(x => x.GetByIdAsync(command.AssignmentId))
            .ReturnsAsync(assignment);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task MarkInProgress_InvalidStateTransition_ThrowsInvalidOperationException()
    {
        // Arrange
        var assignment = CreateAssignment(status: AssignmentStatus.InProgress); // Already in progress
        var command = new UpdateAssignmentStatusCommand(assignment.Id, AssignmentStatus.InProgress, assignment.ContractorId);

        _mockAssignmentRepository
            .Setup(x => x.GetByIdAsync(command.AssignmentId))
            .ReturnsAsync(assignment);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task MarkComplete_NotInProgressStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var assignment = CreateAssignment(status: AssignmentStatus.Accepted); // Not in progress
        var command = new UpdateAssignmentStatusCommand(assignment.Id, AssignmentStatus.Completed, assignment.ContractorId);

        _mockAssignmentRepository
            .Setup(x => x.GetByIdAsync(command.AssignmentId))
            .ReturnsAsync(assignment);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task MarkInProgress_AssignmentHasNoJob_ThrowsInvalidOperationException()
    {
        // Arrange
        var assignment = CreateAssignment();
        assignment.Job = null; // No associated job

        var command = new UpdateAssignmentStatusCommand(assignment.Id, AssignmentStatus.InProgress, assignment.ContractorId);

        _mockAssignmentRepository
            .Setup(x => x.GetByIdAsync(command.AssignmentId))
            .ReturnsAsync(assignment);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task MarkComplete_PublishesJobCompletedEventWithCorrectData()
    {
        // Arrange
        var assignment = CreateAssignment(status: AssignmentStatus.InProgress);
        assignment.StartedAt = DateTime.UtcNow.AddHours(-1);
        assignment.Job!.CustomerId = 200;

        var command = new UpdateAssignmentStatusCommand(assignment.Id, AssignmentStatus.Completed, assignment.ContractorId);

        _mockAssignmentRepository
            .Setup(x => x.GetByIdAsync(command.AssignmentId))
            .ReturnsAsync(assignment);

        _mockAssignmentRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Assignment>()))
            .ReturnsAsync(assignment);

        JobCompletedEvent? publishedEvent = null;
        _mockEventPublisher
            .Setup(x => x.Publish(It.IsAny<JobCompletedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<JobCompletedEvent, CancellationToken>((e, ct) => publishedEvent = e)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        publishedEvent.Should().NotBeNull();
        publishedEvent!.JobId.Should().Be(assignment.JobId);
        publishedEvent.AssignmentId.Should().Be(assignment.Id);
        publishedEvent.ContractorId.Should().Be(assignment.ContractorId);
        publishedEvent.CustomerId.Should().Be(assignment.Job!.CustomerId);
    }

    [Fact]
    public async Task MarkInProgress_ReturnsCorrectAssignmentDto()
    {
        // Arrange
        var assignment = CreateAssignment(status: AssignmentStatus.Accepted);
        var command = new UpdateAssignmentStatusCommand(assignment.Id, AssignmentStatus.InProgress, assignment.ContractorId);

        _mockAssignmentRepository
            .Setup(x => x.GetByIdAsync(command.AssignmentId))
            .ReturnsAsync(assignment);

        _mockAssignmentRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Assignment>()))
            .ReturnsAsync(assignment);

        _mockEventPublisher
            .Setup(x => x.Publish(It.IsAny<JobInProgressEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeOfType<AssignmentDto>();
        result.Id.Should().Be(assignment.Id);
        result.JobId.Should().Be(assignment.JobId);
        result.ContractorId.Should().Be(assignment.ContractorId);
        result.Status.Should().Be("InProgress");
    }
}


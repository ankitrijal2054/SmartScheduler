using MediatR;
using Microsoft.Extensions.Logging;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Domain.Events;
using SmartScheduler.Domain.Exceptions;

namespace SmartScheduler.Application.Commands;

/// <summary>
/// Handler for UpdateAssignmentStatusCommand.
/// Manages status transitions for assignments (Accepted → InProgress → Completed).
/// Publishes domain events to trigger email notifications and real-time updates.
/// </summary>
public class UpdateAssignmentStatusCommandHandler : IRequestHandler<UpdateAssignmentStatusCommand, AssignmentDto>
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IPublisher _eventPublisher;
    private readonly ILogger<UpdateAssignmentStatusCommandHandler> _logger;

    public UpdateAssignmentStatusCommandHandler(
        IAssignmentRepository assignmentRepository,
        IPublisher eventPublisher,
        ILogger<UpdateAssignmentStatusCommandHandler> logger)
    {
        _assignmentRepository = assignmentRepository ?? throw new ArgumentNullException(nameof(assignmentRepository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the command to update an assignment's status.
    /// Process:
    /// 1. Validates assignment exists
    /// 2. Checks authorization (contractor can only update their own assignments)
    /// 3. Calls domain method to transition status (validates transition rules)
    /// 4. Persists the updated assignment
    /// 5. Publishes domain event (JobInProgressEvent or JobCompletedEvent)
    /// 6. Returns updated assignment as DTO
    /// </summary>
    public async Task<AssignmentDto> Handle(UpdateAssignmentStatusCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "Processing UpdateAssignmentStatusCommand for assignment {AssignmentId}, new status {NewStatus}, contractor {ContractorId}",
            request.AssignmentId, request.NewStatus, request.ContractorId);

        try
        {
            // Get the assignment
            var assignment = await _assignmentRepository.GetByIdAsync(request.AssignmentId);
            if (assignment == null)
            {
                _logger.LogWarning("Assignment {AssignmentId} not found", request.AssignmentId);
                throw new AssignmentNotFoundException($"Assignment with ID {request.AssignmentId} not found");
            }

            // Authorization: contractor can only update their own assignments
            if (assignment.ContractorId != request.ContractorId)
            {
                _logger.LogWarning(
                    "Unauthorized attempt to update assignment {AssignmentId}. " +
                    "Contractor {ContractorId} is not the assigned contractor {AssignedContractorId}",
                    request.AssignmentId, request.ContractorId, assignment.ContractorId);
                throw new UnauthorizedAccessException(
                    $"Contractor {request.ContractorId} is not authorized to update assignment {request.AssignmentId}");
            }

            // Get job to retrieve CustomerId for event publishing
            if (assignment.Job == null)
            {
                _logger.LogWarning("Assignment {AssignmentId} has no associated job", request.AssignmentId);
                throw new InvalidOperationException($"Assignment {request.AssignmentId} has no associated job");
            }

            var previousStatus = assignment.Status;

            // Call domain method to transition status (validates transition rules)
            if (request.NewStatus == AssignmentStatus.InProgress)
            {
                assignment.MarkInProgress();
                _logger.LogDebug("Assignment {AssignmentId} marked as InProgress", request.AssignmentId);
            }
            else if (request.NewStatus == AssignmentStatus.Completed)
            {
                assignment.MarkComplete();
                _logger.LogDebug("Assignment {AssignmentId} marked as Completed", request.AssignmentId);
            }
            else
            {
                _logger.LogWarning(
                    "Invalid status transition requested for assignment {AssignmentId}: {PreviousStatus} → {NewStatus}",
                    request.AssignmentId, previousStatus, request.NewStatus);
                throw new InvalidOperationException(
                    $"Invalid status transition from {previousStatus} to {request.NewStatus}");
            }

            // Persist the updated assignment
            var updatedAssignment = await _assignmentRepository.UpdateAsync(assignment);
            _logger.LogInformation(
                "Assignment {AssignmentId} updated successfully. Status: {PreviousStatus} → {NewStatus}",
                request.AssignmentId, previousStatus, updatedAssignment.Status);

            // Publish domain event based on new status
            if (updatedAssignment.Status == AssignmentStatus.InProgress)
            {
                var inProgressEvent = new JobInProgressEvent(
                    assignment.JobId,
                    assignment.Id,
                    assignment.ContractorId,
                    assignment.Job.CustomerId);

                await _eventPublisher.Publish(inProgressEvent, cancellationToken);
                _logger.LogInformation(
                    "JobInProgressEvent published for assignment {AssignmentId}, customer {CustomerId}",
                    request.AssignmentId, assignment.Job.CustomerId);
            }
            else if (updatedAssignment.Status == AssignmentStatus.Completed)
            {
                var completedEvent = new JobCompletedEvent(
                    assignment.JobId,
                    assignment.Id,
                    assignment.ContractorId,
                    assignment.Job.CustomerId);

                await _eventPublisher.Publish(completedEvent, cancellationToken);
                _logger.LogInformation(
                    "JobCompletedEvent published for assignment {AssignmentId}, customer {CustomerId}",
                    request.AssignmentId, assignment.Job.CustomerId);
            }

            // Return updated assignment as DTO
            var result = new AssignmentDto
            {
                Id = updatedAssignment.Id,
                JobId = updatedAssignment.JobId,
                ContractorId = updatedAssignment.ContractorId,
                Status = updatedAssignment.Status.ToString(),
                AssignedAt = updatedAssignment.AssignedAt,
                AcceptedAt = updatedAssignment.AcceptedAt,
                DeclinedAt = updatedAssignment.DeclinedAt,
                StartedAt = updatedAssignment.StartedAt,
                CompletedAt = updatedAssignment.CompletedAt
            };

            return result;
        }
        catch (InvalidOperationException)
        {
            throw; // Domain validation exceptions
        }
        catch (UnauthorizedAccessException)
        {
            throw; // Authorization exceptions
        }
        catch (DomainException)
        {
            throw; // Other domain exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing UpdateAssignmentStatusCommand for assignment {AssignmentId}, contractor {ContractorId}",
                request.AssignmentId, request.ContractorId);
            throw;
        }
    }
}


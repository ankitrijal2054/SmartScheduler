using MediatR;
using Microsoft.Extensions.Logging;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Domain.Exceptions;

namespace SmartScheduler.Application.Commands;

/// <summary>
/// Handler for AcceptAssignmentCommand.
/// Manages accepting pending assignments.
/// </summary>
public class AcceptAssignmentCommandHandler : IRequestHandler<AcceptAssignmentCommand, AssignmentDto>
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ILogger<AcceptAssignmentCommandHandler> _logger;

    public AcceptAssignmentCommandHandler(
        IAssignmentRepository assignmentRepository,
        ILogger<AcceptAssignmentCommandHandler> logger)
    {
        _assignmentRepository = assignmentRepository ?? throw new ArgumentNullException(nameof(assignmentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AssignmentDto> Handle(AcceptAssignmentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "Processing AcceptAssignmentCommand for assignment {AssignmentId}, contractor {ContractorId}",
            request.AssignmentId, request.ContractorId);

        try
        {
            // Get the assignment
            var assignment = await _assignmentRepository.GetByIdAsync(request.AssignmentId);
            if (assignment == null)
            {
                _logger.LogWarning("Assignment {AssignmentId} not found", request.AssignmentId);
                throw new AssignmentNotFoundException($"Assignment with ID {request.AssignmentId} not found");
            }

            // Authorization: contractor can only accept their own assignments
            if (assignment.ContractorId != request.ContractorId)
            {
                _logger.LogWarning(
                    "Unauthorized attempt to accept assignment {AssignmentId}. " +
                    "Contractor {ContractorId} is not the assigned contractor {AssignedContractorId}",
                    request.AssignmentId, request.ContractorId, assignment.ContractorId);
                throw new UnauthorizedAccessException(
                    $"Contractor {request.ContractorId} is not authorized to accept assignment {request.AssignmentId}");
            }

            // Accept the assignment (validates status)
            assignment.Accept();

            // Persist the updated assignment
            var updatedAssignment = await _assignmentRepository.UpdateAsync(assignment);
            _logger.LogInformation(
                "Assignment {AssignmentId} accepted successfully by contractor {ContractorId}",
                request.AssignmentId, request.ContractorId);

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
                "Error processing AcceptAssignmentCommand for assignment {AssignmentId}, contractor {ContractorId}",
                request.AssignmentId, request.ContractorId);
            throw;
        }
    }
}


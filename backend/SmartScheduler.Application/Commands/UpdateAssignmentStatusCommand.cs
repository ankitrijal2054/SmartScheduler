using MediatR;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Domain.Enums;

namespace SmartScheduler.Application.Commands;

/// <summary>
/// Command to update the status of a job assignment.
/// Used by contractors to mark jobs as in-progress or completed.
/// </summary>
public class UpdateAssignmentStatusCommand : IRequest<AssignmentDto>
{
    /// <summary>
    /// Gets the assignment ID to update.
    /// </summary>
    public int AssignmentId { get; }

    /// <summary>
    /// Gets the new status to transition to.
    /// </summary>
    public AssignmentStatus NewStatus { get; }

    /// <summary>
    /// Gets the ID of the contractor performing the update.
    /// Used for authorization (contractor can only update their own assignments).
    /// </summary>
    public int ContractorId { get; }

    public UpdateAssignmentStatusCommand(int assignmentId, AssignmentStatus newStatus, int contractorId)
    {
        AssignmentId = assignmentId;
        NewStatus = newStatus;
        ContractorId = contractorId;
    }
}


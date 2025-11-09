using MediatR;
using SmartScheduler.Application.DTOs;

namespace SmartScheduler.Application.Commands;

/// <summary>
/// Command to decline a job assignment.
/// Used by contractors to decline pending job assignments.
/// </summary>
public class DeclineAssignmentCommand : IRequest<AssignmentDto>
{
    /// <summary>
    /// Gets the assignment ID to decline.
    /// </summary>
    public int AssignmentId { get; }

    /// <summary>
    /// Gets the ID of the contractor performing the decline action.
    /// Used for authorization (contractor can only decline their own assignments).
    /// </summary>
    public int ContractorId { get; }

    /// <summary>
    /// Gets the optional reason for declining the assignment.
    /// </summary>
    public string? Reason { get; }

    public DeclineAssignmentCommand(int assignmentId, int contractorId, string? reason = null)
    {
        AssignmentId = assignmentId;
        ContractorId = contractorId;
        Reason = reason;
    }
}


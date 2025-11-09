using MediatR;
using SmartScheduler.Application.DTOs;

namespace SmartScheduler.Application.Commands;

/// <summary>
/// Command to accept a job assignment.
/// Used by contractors to accept pending job assignments.
/// </summary>
public class AcceptAssignmentCommand : IRequest<AssignmentDto>
{
    /// <summary>
    /// Gets the assignment ID to accept.
    /// </summary>
    public int AssignmentId { get; }

    /// <summary>
    /// Gets the ID of the contractor performing the accept action.
    /// Used for authorization (contractor can only accept their own assignments).
    /// </summary>
    public int ContractorId { get; }

    public AcceptAssignmentCommand(int assignmentId, int contractorId)
    {
        AssignmentId = assignmentId;
        ContractorId = contractorId;
    }
}


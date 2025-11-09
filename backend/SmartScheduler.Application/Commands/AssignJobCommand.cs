using MediatR;

namespace SmartScheduler.Application.Commands;

/// <summary>
/// Command to assign a job to a contractor.
/// Used by dispatchers to assign jobs after reviewing recommendations.
/// </summary>
public class AssignJobCommand : IRequest<int>
{
    /// <summary>
    /// Gets the job ID to assign.
    /// </summary>
    public int JobId { get; }

    /// <summary>
    /// Gets the contractor ID to assign to.
    /// </summary>
    public int ContractorId { get; }

    /// <summary>
    /// Gets the dispatcher ID performing the assignment.
    /// </summary>
    public int DispatcherId { get; }

    public AssignJobCommand(int jobId, int contractorId, int dispatcherId)
    {
        JobId = jobId;
        ContractorId = contractorId;
        DispatcherId = dispatcherId;
    }
}


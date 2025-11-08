using MediatR;

namespace SmartScheduler.Application.Commands;

/// <summary>
/// Command to remove a contractor from dispatcher's curated list.
/// Idempotent: Removing a contractor not in list returns success without error.
/// </summary>
public class RemoveContractorFromListCommand : IRequest<Unit>
{
    /// <summary>
    /// The ID of the dispatcher removing the contractor.
    /// Extracted from JWT token for security.
    /// </summary>
    public int DispatcherId { get; set; }

    /// <summary>
    /// The ID of the contractor to remove.
    /// </summary>
    public int ContractorId { get; set; }

    public RemoveContractorFromListCommand()
    {
    }

    public RemoveContractorFromListCommand(int dispatcherId, int contractorId)
    {
        DispatcherId = dispatcherId;
        ContractorId = contractorId;
    }
}


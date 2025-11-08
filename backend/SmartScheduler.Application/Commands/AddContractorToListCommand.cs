using MediatR;

namespace SmartScheduler.Application.Commands;

/// <summary>
/// Command to add a contractor to dispatcher's curated list.
/// Idempotent: Adding same contractor twice returns success without error.
/// </summary>
public class AddContractorToListCommand : IRequest<int>
{
    /// <summary>
    /// The ID of the dispatcher adding the contractor.
    /// Extracted from JWT token for security.
    /// </summary>
    public int DispatcherId { get; set; }

    /// <summary>
    /// The ID of the contractor to add.
    /// </summary>
    public int ContractorId { get; set; }

    public AddContractorToListCommand()
    {
    }

    public AddContractorToListCommand(int dispatcherId, int contractorId)
    {
        DispatcherId = dispatcherId;
        ContractorId = contractorId;
    }
}


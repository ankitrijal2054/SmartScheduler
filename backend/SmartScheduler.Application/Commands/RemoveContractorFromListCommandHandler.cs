using MediatR;
using SmartScheduler.Application.Repositories;

namespace SmartScheduler.Application.Commands;

/// <summary>
/// Handler for RemoveContractorFromListCommand.
/// Removes a contractor from dispatcher's curated list (idempotent).
/// </summary>
public class RemoveContractorFromListCommandHandler : IRequestHandler<RemoveContractorFromListCommand, Unit>
{
    private readonly IDispatcherContractorListRepository _repository;

    public RemoveContractorFromListCommandHandler(IDispatcherContractorListRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Handles the command to remove contractor from dispatcher's list.
    /// Idempotent: No error if contractor not in list.
    /// </summary>
    public async Task<Unit> Handle(RemoveContractorFromListCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Remove from list (idempotent - no error if not found)
        await _repository.RemoveAsync(request.DispatcherId, request.ContractorId);
        return Unit.Value;
    }
}


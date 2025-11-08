using MediatR;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Domain.Exceptions;

namespace SmartScheduler.Application.Commands;

/// <summary>
/// Handler for AddContractorToListCommand.
/// Adds a contractor to dispatcher's curated list (idempotent).
/// </summary>
public class AddContractorToListCommandHandler : IRequestHandler<AddContractorToListCommand, int>
{
    private readonly IDispatcherContractorListRepository _repository;
    private readonly IContractorRepository _contractorRepository;

    public AddContractorToListCommandHandler(
        IDispatcherContractorListRepository repository,
        IContractorRepository contractorRepository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _contractorRepository = contractorRepository ?? throw new ArgumentNullException(nameof(contractorRepository));
    }

    /// <summary>
    /// Handles the command to add contractor to dispatcher's list.
    /// </summary>
    public async Task<int> Handle(AddContractorToListCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verify contractor exists
        var contractor = await _contractorRepository.GetByIdAsync(request.ContractorId);
        if (contractor == null)
        {
            throw new NotFoundException($"Contractor with ID {request.ContractorId} not found");
        }

        // Add to list (idempotent - returns existing if already present)
        var result = await _repository.AddAsync(request.DispatcherId, request.ContractorId);
        return result.Id;
    }
}


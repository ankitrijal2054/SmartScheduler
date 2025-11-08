using FluentValidation;
using SmartScheduler.Application.Commands;

namespace SmartScheduler.Application.Validators;

/// <summary>
/// Validator for RemoveContractorFromListCommand.
/// </summary>
public class RemoveContractorFromListCommandValidator : AbstractValidator<RemoveContractorFromListCommand>
{
    public RemoveContractorFromListCommandValidator()
    {
        RuleFor(x => x.DispatcherId)
            .GreaterThan(0).WithMessage("Dispatcher ID must be greater than 0");

        RuleFor(x => x.ContractorId)
            .GreaterThan(0).WithMessage("Contractor ID must be greater than 0");
    }
}


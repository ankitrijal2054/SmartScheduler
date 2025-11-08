using FluentValidation;
using SmartScheduler.Application.Queries;

namespace SmartScheduler.Application.Validators;

/// <summary>
/// Validator for GetDispatcherContractorListQuery.
/// </summary>
public class GetDispatcherContractorListQueryValidator : AbstractValidator<GetDispatcherContractorListQuery>
{
    public GetDispatcherContractorListQueryValidator()
    {
        RuleFor(x => x.DispatcherId)
            .GreaterThan(0).WithMessage("Dispatcher ID must be greater than 0");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Limit cannot exceed 100 items per page");

        RuleFor(x => x.Search)
            .MaximumLength(100).WithMessage("Search filter cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Search));
    }
}


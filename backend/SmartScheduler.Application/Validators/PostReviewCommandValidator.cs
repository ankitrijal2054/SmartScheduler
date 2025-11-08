using FluentValidation;
using SmartScheduler.Application.Commands;

namespace SmartScheduler.Application.Validators;

/// <summary>
/// Validator for PostReviewCommand.
/// Validates rating is in valid range and comment length constraints.
/// </summary>
public class PostReviewCommandValidator : AbstractValidator<PostReviewCommand>
{
    public PostReviewCommandValidator()
    {
        RuleFor(x => x.JobId)
            .GreaterThan(0)
            .WithMessage("Job ID must be greater than 0");

        RuleFor(x => x.ContractorId)
            .GreaterThan(0)
            .WithMessage("Contractor ID must be greater than 0");

        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithMessage("Customer ID must be greater than 0");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5 stars inclusive");

        RuleFor(x => x.Comment)
            .MaximumLength(500)
            .WithMessage("Comment cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Comment));
    }
}


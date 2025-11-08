using FluentValidation;
using SmartScheduler.Application.DTOs;

namespace SmartScheduler.Application.Validators;

/// <summary>
/// Validator for CreateContractorRequest DTOs.
/// </summary>
public class CreateContractorRequestValidator : AbstractValidator<CreateContractorRequest>
{
    public CreateContractorRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(2, 100).WithMessage("Name must be between 2 and 100 characters");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required")
            .Length(5, 200).WithMessage("Location must be between 5 and 200 characters");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .Matches(@"^\+?[1-9]\d{1,14}$|^[0-9]{3}[-.\s]?[0-9]{3}[-.\s]?[0-9]{4}$")
            .WithMessage("Invalid phone format. Use E.164 format (+1234567890) or standard format (123-456-7890)");

        RuleFor(x => x.TradeType)
            .NotEmpty().WithMessage("TradeType is required")
            .Must(IsValidTradeType).WithMessage("Invalid TradeType. Valid types: Flooring, HVAC, Plumbing, Electrical, Other");

        RuleFor(x => x.WorkingHours)
            .NotNull().WithMessage("WorkingHours is required")
            .SetValidator(new CreateWorkingHoursRequestValidator());
    }

    private static bool IsValidTradeType(string? tradeType)
    {
        if (string.IsNullOrEmpty(tradeType))
            return false;

        var validTypes = new[] { "Flooring", "HVAC", "Plumbing", "Electrical", "Other" };
        return validTypes.Contains(tradeType, StringComparer.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Validator for working hours.
/// </summary>
public class CreateWorkingHoursRequestValidator : AbstractValidator<CreateWorkingHoursRequest>
{
    public CreateWorkingHoursRequestValidator()
    {
        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("StartTime is required")
            .Must(IsValidTimeFormat).WithMessage("StartTime must be in HH:mm or HH:mm:ss format");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("EndTime is required")
            .Must(IsValidTimeFormat).WithMessage("EndTime must be in HH:mm or HH:mm:ss format");

        RuleFor(x => x)
            .Must(IsEndTimeAfterStartTime).WithMessage("EndTime must be after StartTime");

        RuleFor(x => x.WorkDays)
            .NotEmpty().WithMessage("At least one work day is required")
            .Must(ValidateWorkDays).WithMessage("Invalid work days. Valid days: Mon, Tue, Wed, Thu, Fri, Sat, Sun");
    }

    private static bool IsValidTimeFormat(string? time)
    {
        if (string.IsNullOrEmpty(time))
            return false;

        return TimeSpan.TryParse(time, out _);
    }

    private static bool IsEndTimeAfterStartTime(CreateWorkingHoursRequest request)
    {
        if (string.IsNullOrEmpty(request.StartTime) || string.IsNullOrEmpty(request.EndTime))
            return true; // Let other validators handle empty values

        if (!TimeSpan.TryParse(request.StartTime, out var startTime) ||
            !TimeSpan.TryParse(request.EndTime, out var endTime))
        {
            return true; // Let other validators handle invalid formats
        }

        return endTime > startTime;
    }

    private static bool ValidateWorkDays(string[]? workDays)
    {
        if (workDays == null || workDays.Length == 0)
            return false;

        var validDays = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        return workDays.All(day => validDays.Contains(day));
    }
}

/// <summary>
/// Validator for UpdateContractorRequest DTOs.
/// All fields are optional, but if provided, they must be valid.
/// </summary>
public class UpdateContractorRequestValidator : AbstractValidator<UpdateContractorRequest>
{
    public UpdateContractorRequestValidator()
    {
        RuleFor(x => x.Name)
            .Length(2, 100).WithMessage("Name must be between 2 and 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Location)
            .Length(5, 200).WithMessage("Location must be between 5 and 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Location));

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[1-9]\d{1,14}$|^[0-9]{3}[-.\s]?[0-9]{3}[-.\s]?[0-9]{4}$")
            .WithMessage("Invalid phone format. Use E.164 format (+1234567890) or standard format (123-456-7890)")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.TradeType)
            .Must(IsValidTradeType).WithMessage("Invalid TradeType. Valid types: Flooring, HVAC, Plumbing, Electrical, Other")
            .When(x => !string.IsNullOrEmpty(x.TradeType));

        RuleFor(x => x.WorkingHours!)
            .SetValidator(new CreateWorkingHoursRequestValidator()!)
            .When(x => x.WorkingHours != null);
    }

    private static bool IsValidTradeType(string? tradeType)
    {
        if (string.IsNullOrEmpty(tradeType))
            return true; // Optional field

        var validTypes = new[] { "Flooring", "HVAC", "Plumbing", "Electrical", "Other" };
        return validTypes.Contains(tradeType, StringComparer.OrdinalIgnoreCase);
    }
}


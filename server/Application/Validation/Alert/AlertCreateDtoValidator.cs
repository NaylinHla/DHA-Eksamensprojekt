using Application.Models.Dtos.RestDtos;
using FluentValidation;

namespace Application.Validation.Alert;

public sealed class AlertCreateDtoValidator : AbstractValidator<AlertCreateDto>
{
    public AlertCreateDtoValidator()
    {
        RuleFor(x => x.AlertName)
            .NotEmpty().WithMessage("Alert Name cannot be null")
            .MaximumLength(100).WithMessage("Alert Name cannot be longer than 100 characters");

        RuleFor(x => x.AlertDesc)
            .NotEmpty().WithMessage("Alert Description cannot be empty")
            .MinimumLength(5).WithMessage("Alert Description cannot be shorter than 5 characters");
        
        RuleFor(x => x.AlertConditionId)
            .NotNull().WithMessage("AlertConditionId cannot be null");
        
        RuleFor(x => x.AlertUser)
            .NotNull().WithMessage("AlertUser cannot be null");
    }
}
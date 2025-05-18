using Application.Models.Dtos.RestDtos;
using FluentValidation;

namespace Application.Validation.AlertCondition;

public sealed class ConditionAlertPlantCreateDtoValidator : AbstractValidator<ConditionAlertPlantCreateDto>
{
    public ConditionAlertPlantCreateDtoValidator()
    {
        RuleFor(x => x.PlantId)
            .NotEmpty().WithMessage("PlantId is required")
            .NotEqual(Guid.Empty).WithMessage("PlantId must be a non-empty GUID");
    }
}
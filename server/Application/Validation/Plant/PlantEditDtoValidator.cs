using Application.Models.Dtos.RestDtos;
using FluentValidation;

namespace Application.Validation.Plant;

public sealed class PlantEditDtoValidator : AbstractValidator<PlantEditDto>
{
    public PlantEditDtoValidator()
    {
        RuleFor(x => x.PlantName)
            .NotEmpty().WithMessage("PlantName cannot be empty")
            .MaximumLength(100).WithMessage("PlantName cannot be longer than 100 characters");
        
        RuleFor(x => x.PlantType)
            .NotEmpty().WithMessage("PlantType cannot be empty")
            .MaximumLength(50).WithMessage("PlantType cannot be longer than 50 characters");
        
        RuleFor(x => x.WaterEvery)
            .GreaterThan(0).WithMessage("Water Every cannot be less than 1 day")
            .LessThanOrEqualTo(365).WithMessage("Water Every cannot be more than 365 days")
            .When(x => x.WaterEvery is not null);
        
        RuleFor(x => x.PlantNotes)
            .MaximumLength(1000).WithMessage("PlantNotes cannot be longer than 1000 characters");
        
        RuleFor(x => x.LastWatered)
            .LessThanOrEqualTo(DateTime.UtcNow.AddSeconds(1)).WithMessage("LastWatered cannot be in the future (Margin of 1 second)")
            .When(x => x.LastWatered.HasValue);
    }
}
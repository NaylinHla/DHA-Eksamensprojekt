using Application.Models.Dtos.RestDtos;
using FluentValidation;

namespace Application.Validation.Plant;

public sealed class PlantCreateDtoValidator : AbstractValidator<PlantCreateDto>
{
    public PlantCreateDtoValidator()
    {
        RuleFor(x => x.PlantName)
            .NotEmpty().WithMessage("PlantName cannot be empty")
            .MaximumLength(100).WithMessage("PlantName cannot be longer than 100 characters");
        
        RuleFor(x => x.PlantType)
            .NotEmpty().WithMessage("PlantType cannot be empty")
            .MaximumLength(50).WithMessage("PlantType cannot be longer than 50 characters");

        RuleFor(x => x.WaterEvery)
            .GreaterThan(0).WithMessage("Water Every cannot be less than 1 day")
            .LessThanOrEqualTo(365).WithMessage("Water Every cannot be bigger than 365 days")
            .When(x => x.WaterEvery is not null);

        RuleFor(x => x.Planted)
            .LessThanOrEqualTo(DateTime.UtcNow.AddSeconds(1)).WithMessage("Planted cannot be in the future (One second margin allowed)")
            .When(x => x.Planted.HasValue);

        RuleFor(x => x.PlantNotes)
            .MaximumLength(1000).WithMessage("PlantNotes cannot be longer than 1000 characters");
        
        RuleFor(x => x.IsDead)
            .NotEqual(true).WithMessage("Is Dead cannot be true when you are creating a plant");
    }
}
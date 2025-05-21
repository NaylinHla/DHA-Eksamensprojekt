using Application.Models.Dtos.RestDtos;
using FluentValidation;

namespace Application.Validation.Alert;

public sealed class IsAlertUserDeviceConditionMeetDtoValidator : AbstractValidator<IsAlertUserDeviceConditionMeetDto>
{
    public IsAlertUserDeviceConditionMeetDtoValidator()
    {
        RuleFor(x => x.UserDeviceId)
            .NotEmpty().WithMessage("UserDeviceId cannot be empty")
            .MaximumLength(1000).WithMessage("UserDeviceId cannot exceed 1000 characters")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("UserDeviceId must be a valid GUID");

        RuleFor(x => x.Temperature)
            .InclusiveBetween(-100, 150).When(x => x.Temperature.HasValue)
            .WithMessage("Temperature must be between -100 and 150");

        RuleFor(x => x.Humidity)
            .InclusiveBetween(0, 100).When(x => x.Humidity.HasValue)
            .WithMessage("Humidity must be between 0 and 100");

        RuleFor(x => x.AirPressure)
            .GreaterThan(0).When(x => x.AirPressure.HasValue)
            .WithMessage("AirPressure must be greater than 0");

        RuleFor(x => x.AirQuality)
            .InclusiveBetween(0, 2000).When(x => x.AirQuality.HasValue)
            .WithMessage("AirQuality must be between 0 and 2000");

        RuleFor(x => x.Time)
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(1))
            .WithMessage("Time cannot be in the future");
    }
}
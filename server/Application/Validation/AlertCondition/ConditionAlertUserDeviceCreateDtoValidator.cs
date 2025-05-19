using Application.Models.Dtos.RestDtos;
using FluentValidation;

namespace Application.Validation.AlertCondition;

public sealed class ConditionAlertUserDeviceCreateDtoValidator : AbstractValidator<ConditionAlertUserDeviceCreateDto>
{
    private static readonly string[] AllowedSensorTypes =
        ["Temperature", "Humidity", "AirPressure", "AirQuality"];
    
    public ConditionAlertUserDeviceCreateDtoValidator()
    {
        RuleFor(x => x.UserDeviceId)
            .NotEmpty().WithMessage("UserDeviceId is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("UserDeviceId must be a valid GUID")
            .MaximumLength(1000);

        RuleFor(x => x.SensorType)
            .NotEmpty().WithMessage("SensorType is required")
            .Must(s => AllowedSensorTypes.Contains(s))
            .WithMessage("SensorType must be one of: Temperature, Humidity, AirPressure, AirQuality")
            .MaximumLength(50);

        RuleFor(x => x.Condition)
            .NotEmpty().WithMessage("Condition is required")
            .Matches(@"^(<=|>=)\d+(\.\d+)?$")
            .WithMessage("Condition must be in the format '<=Number' or '>=Number'")
            .MaximumLength(20);
    }
}
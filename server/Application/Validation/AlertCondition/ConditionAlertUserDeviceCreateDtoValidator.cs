using Application.Models.Dtos.RestDtos;
using FluentValidation;

namespace Application.Validation.AlertCondition;

public sealed class ConditionAlertUserDeviceCreateDtoValidator : AbstractValidator<ConditionAlertUserDeviceCreateDto>
{
    public ConditionAlertUserDeviceCreateDtoValidator()
    {
        RuleFor(x => x.UserDeviceId)
            .NotEmpty().WithMessage("UserDeviceId is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("UserDeviceId must be a valid GUID")
            .MaximumLength(1000);

        RuleFor(x => x.SensorType)
            .NotEmpty().WithMessage("SensorType is required")
            .Must(GlobalValidator.IsValidSensorType)
            .WithMessage("SensorType must be one of: Temperature, Humidity, AirPressure, AirQuality")
            .MaximumLength(50);

        RuleFor(x => x.Condition)
            .NotEmpty().WithMessage("Condition is required")
            .Must((dto, condition) => 
                GlobalValidator.IsConditionFormatValid(condition, dto.SensorType) 
                && GlobalValidator.IsConditionValueInRange(dto.SensorType, condition))
            .WithMessage("Condition must be valid and within the allowed sensor range")
            .MaximumLength(15);


        RuleFor(x => x)
            .Must(dto => GlobalValidator.IsConditionValueInRange(dto.SensorType, dto.Condition))
            .WithMessage("Condition value is out of range for the selected SensorType");
    }
}
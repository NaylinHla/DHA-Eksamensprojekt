using Application.Models.Dtos.MqttSubscriptionDto;
using FluentValidation;

namespace Application.Validation.MQTT;

public sealed class DeviceSensorDataDtoValidator : AbstractValidator<DeviceSensorDataDto>
{
    public DeviceSensorDataDtoValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("DeviceId cannot be empty")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("DeviceId must be a valid Guid");

        RuleFor(x => x.Temperature)
            .InclusiveBetween(-40, 130).WithMessage("Temperature must be between -40 and 130");
        
        RuleFor(x => x.Humidity)
            .InclusiveBetween(0, 100).WithMessage("Humidity must be between 0 and 100");
        
        RuleFor(x => x.AirPressure)
            .GreaterThan(0).WithMessage("Air Pressure must be greater than 0");
        
        RuleFor(x => x.AirQuality)
            .InclusiveBetween(0, 2000).WithMessage("Air Quality must be between 0 and 2000");
        
        RuleFor(x => x.Time)
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(1)).WithMessage("Time cannot be in the future.");
    }
}
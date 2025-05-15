using Application.Models.Dtos.RestDtos.UserDevice.Request;
using FluentValidation;

namespace Application.Validation.UserDevice;

public sealed class UserDeviceCreateDtoValidator : AbstractValidator<UserDeviceCreateDto>
{
    public UserDeviceCreateDtoValidator()
    {
        RuleFor(x => x.DeviceName)
            .NotEmpty().WithMessage("DeviceName cannot be empty")
            .MinimumLength(2).WithMessage("DeviceName must be at least 2 characters.")
            .MaximumLength(50).WithMessage("DeviceName cannot be longer than 50 characters.");
        
        RuleFor(x => x.DeviceDescription)
            .MaximumLength(500).WithMessage("DeviceDescription cannot be longer than 500 characters.")
            .When(x => x.DeviceDescription is not null);
        
        RuleFor(x => x.WaitTime)
            .Matches("^[0-9]{1,5}$")
            .When(x => x.WaitTime is not null)
            .WithMessage("WaitTime must be numeric seconds.");
        
        RuleFor(x => x.Created)
            .NotNull().WithMessage("Created cannot be null")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Created cannot be in the future.");
    }
}
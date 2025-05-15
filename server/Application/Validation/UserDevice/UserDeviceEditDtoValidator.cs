using Application.Models.Dtos.RestDtos.UserDevice.Request;
using FluentValidation;

namespace Application.Validation.UserDevice;

public sealed class UserDeviceEditDtoValidator : AbstractValidator<UserDeviceEditDto>
{
    public UserDeviceEditDtoValidator()
    {
        RuleFor(x => x.DeviceName)
            .MaximumLength(50).WithMessage("DeviceName cannot be longer than 50 characters.")
            .When(x => x.DeviceName is not null);
        
        RuleFor(x => x.DeviceDescription)
            .MaximumLength(500).WithMessage("DeviceDescription cannot be longer than 500 characters.")
            .When(x => x.DeviceDescription is not null);
        
        RuleFor(x => x.WaitTime)
            .Matches("^[0-9]{1,5}$")
            .When(x => x.WaitTime is not null);
    }
}
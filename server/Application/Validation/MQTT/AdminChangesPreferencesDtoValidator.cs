using Application.Models.Dtos.RestDtos;
using FluentValidation;

namespace Application.Validation.MQTT;

public sealed class AdminChangesPreferencesDtoValidator : AbstractValidator<AdminChangesPreferencesDto>
{
    public AdminChangesPreferencesDtoValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("DeviceId cannot be empty");
        
        RuleFor(x => x.Interval)
            .Matches("^[0-9]{1,5}$")
            .When(x => x.Interval is not null);
    }
}
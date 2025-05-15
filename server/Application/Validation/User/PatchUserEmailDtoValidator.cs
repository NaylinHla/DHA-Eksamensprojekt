using Application.Models.Dtos.RestDtos.Request;
using FluentValidation;

namespace Application.Validation.User;

public sealed class PatchUserEmailDtoValidator : AbstractValidator<PatchUserEmailDto>
{
    public PatchUserEmailDtoValidator()
    {
        RuleFor(x => x.OldEmail)
            .NotEmpty().WithMessage("Email cannot be empty")
            .EmailAddress().WithMessage("Email is not valid");
        
        RuleFor(x => x.NewEmail)
            .NotEmpty().WithMessage("Email cannot be empty")
            .EmailAddress().WithMessage("Email is not valid");
    }
}
using Application.Models.Dtos.RestDtos.Request;
using FluentValidation;

namespace Application.Validation.User;

public sealed class PatchUserPasswordDtoValidator : AbstractValidator<PatchUserPasswordDto>
{
    public PatchUserPasswordDtoValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty().WithMessage("Password cannot be empty")
            .MinimumLength(4).WithMessage("Password must be at least 4 characters.");
        
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Password cannot be empty")
            .MinimumLength(4).WithMessage("Password must be at least 4 characters.");
    }
}
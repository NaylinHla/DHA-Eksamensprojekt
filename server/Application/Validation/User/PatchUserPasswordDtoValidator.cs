using Application.Models.Dtos.RestDtos.Request;
using FluentValidation;

namespace Application.Validation.User;

public sealed class PatchUserPasswordDtoValidator : AbstractValidator<PatchUserPasswordDto>
{
    public PatchUserPasswordDtoValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty().WithMessage("Password cannot be empty")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).{6,}$")
            .WithMessage("Password must be at least 6 characters and include uppercase, lowercase, number, and special character.");
        
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Password cannot be empty")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).{6,}$")
            .WithMessage("Password must be at least 6 characters and include uppercase, lowercase, number, and special character.");
    }
}
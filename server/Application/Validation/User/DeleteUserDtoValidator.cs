using Application.Models.Dtos.RestDtos.Request;
using FluentValidation;

namespace Application.Validation.User;

public sealed class DeleteUserDtoValidator : AbstractValidator<DeleteUserDto>
{
    public DeleteUserDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email cannot be empty")
            .EmailAddress().WithMessage("Email is not valid");
    }
}
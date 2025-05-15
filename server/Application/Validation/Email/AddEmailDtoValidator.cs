using Application.Models.Dtos.RestDtos.EmailList.Request;
using FluentValidation;

namespace Application.Validation.Email;

public sealed class AddEmailDtoValidator : AbstractValidator<AddEmailDto>
{
    public AddEmailDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email cannot be empty")
            .EmailAddress().WithMessage("Email is not valid");
    }
}
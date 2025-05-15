using Application.Models.Dtos.RestDtos.EmailList.Request;
using FluentValidation;

namespace Application.Validation.Email;

public sealed class RemoveEmailDtoValidator : AbstractValidator<RemoveEmailDto>
{
    public RemoveEmailDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email cannot be empty")
            .EmailAddress().WithMessage("Email is not valid");
    }
}
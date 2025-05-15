using FluentValidation;
using Application.Models.Dtos.RestDtos;

namespace Application.Validation.Auth;

public sealed class AuthRegisterDtoValidator : AbstractValidator<AuthRegisterDto>
{
    public AuthRegisterDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First Name cannot be empty")
            .MinimumLength(2).WithMessage("First Name cannot be shorter than 2 characters")
            .MaximumLength(30).WithMessage("First Name cannot be longer than 30 characters");
        
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last Name cannot be empty")
            .MinimumLength(2).WithMessage("Last Name cannot be shorter than 2 characters")
            .MaximumLength(30).WithMessage("Last Name cannot be longer than 30 characters");
        
        RuleFor(x => x.Birthday)
            .LessThan(DateTime.UtcNow.AddYears(-5)).WithMessage("Birthday is not valid.");
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email cannot be empty")
            .EmailAddress().WithMessage("Email is not valid");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password cannot be empty")
            .NotNull().WithMessage("Password cannot be null")
            .MinimumLength(4).WithMessage("Password cannot be shorter than 4 characters");
        
        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country cannot be empty")
            .MaximumLength(56).WithMessage("Country cannot be longer than 56 characters");
    }
}
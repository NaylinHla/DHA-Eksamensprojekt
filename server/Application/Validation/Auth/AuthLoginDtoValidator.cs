﻿using Application.Models.Dtos.RestDtos;
using FluentValidation;

namespace Application.Validation.Auth;

public sealed class AuthLoginDtoValidator : AbstractValidator<AuthLoginDto>
{
    public AuthLoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email cannot be empty")
            .MinimumLength(7).WithMessage("Email cannot be shorter than 7 characters")
            .EmailAddress().WithMessage("Email is not valid");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password cannot be empty")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).{6,}$")
            .WithMessage("Password must be at least 6 characters and include uppercase, lowercase, number, and special character.");
    }
}
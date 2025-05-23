using Application.Interfaces.Infrastructure.Postgres;
using Application.Models.Dtos.RestDtos.Request;
using FluentValidation;

namespace Application.Validation.User;

public sealed class PatchUserEmailDtoValidator : AbstractValidator<PatchUserEmailDto>
{
    private readonly IUserRepository _userRepository;
    
    public PatchUserEmailDtoValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        
        RuleFor(x => x.OldEmail)
            .NotEmpty().WithMessage("Email cannot be empty")
            .EmailAddress().WithMessage("Email is not valid");
        
        RuleFor(x => x.NewEmail)
            .NotEmpty().WithMessage("Email cannot be empty")
            .EmailAddress().WithMessage("Email is not valid")
            .MustAsync(BeUniqueEmail).WithMessage("Email already used.");
    }
    
    private async Task<bool> BeUniqueEmail(string email, CancellationToken ct) =>
        !await _userRepository.EmailExistsAsync(email, ct);
}
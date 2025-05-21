using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Models.Dtos.RestDtos.Request;
using Core.Domain.Entities;
using FluentValidation;

namespace Application.Services;

public class UserService(
    IUserRepository userRepository, 
    ISecurityService securityService,
    IValidator<DeleteUserDto> deleteUserValidator,
    IValidator<PatchUserEmailDto> patchUserEmailValidator,
    IValidator<PatchUserPasswordDto> patchUserPasswordValidator)
    : IUserService
{
    private const string UserNotFoundMessage = "User not found.";
    
    public async Task<User> GetUserByEmailAsync(string email)
    {
        var user = userRepository.GetUserOrNull(email) 
                   ?? throw new KeyNotFoundException(UserNotFoundMessage);
        return user;
    }
    
    public async Task<User> DeleteUser(DeleteUserDto request)
    {
        await deleteUserValidator.ValidateAndThrowAsync(request);
        var user = userRepository.GetUserOrNull(request.Email)
            ?? throw new KeyNotFoundException(UserNotFoundMessage);

        // Base anonymized email
        const string baseEmail = "Deleted@User.com";
        var updatedEmail = baseEmail;
        var counter = 1;

        // Ensure email is unique
        while (await userRepository.EmailExistsAsync(updatedEmail))
        {
            updatedEmail = $"Deleted{counter}@User.com";
            counter++;
        }

        // Anonymize user
        user.FirstName = "Deleted";
        user.LastName = "User";
        user.Email = updatedEmail;
        user.Country = "-";
        user.Birthday = DateTime.MinValue;
        
        userRepository.UpdateUser(user);
        userRepository.Save();

        return user;
    }

    public async Task<User> PatchUserEmail(PatchUserEmailDto request)
    {
        await patchUserEmailValidator.ValidateAndThrowAsync(request);
        var user = userRepository.GetUserOrNull(request.OldEmail)
            ?? throw new KeyNotFoundException(UserNotFoundMessage);

        user.Email = request.NewEmail;
        
        userRepository.UpdateUser(user);
        userRepository.Save();

        return user;
    }

    public async Task<User> PatchUserPassword(string email, PatchUserPasswordDto request)
    {
        await patchUserPasswordValidator.ValidateAndThrowAsync(request);
        var user = userRepository.GetUserOrNull(email)
            ?? throw new KeyNotFoundException(UserNotFoundMessage);

        securityService.VerifyPasswordOrThrow(request.OldPassword + user.Salt, user.Hash);

        var newSalt = securityService.GenerateSalt();
        var newHash = securityService.HashPassword(request.NewPassword + newSalt);

        user.Salt = newSalt;
        user.Hash = newHash;
        userRepository.UpdateUser(user);
        userRepository.Save();

        return user;
    }
}
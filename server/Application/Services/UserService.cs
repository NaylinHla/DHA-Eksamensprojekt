using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Models.Dtos.RestDtos.Request;
using Application.Services;
using Core.Domain.Entities;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ISecurityService _securityService;

    public UserService(IUserRepository userRepository, ISecurityService securityService)
    {
        _userRepository = userRepository;
        _securityService = securityService;
    }

    public User DeleteUser(DeleteUserDto request)
    {
        var user = _userRepository.GetUserOrNull(request.Email);
        if (user == null)
            throw new KeyNotFoundException("Bruger blev ikke fundet.");

        // Base anonymized email
        string baseEmail = "Deleted@User.com";
        string updatedEmail = baseEmail;
        int counter = 1;

        // Ensure email is unique
        while (_userRepository.EmailExists(updatedEmail))
        { 
            updatedEmail = $"Deleted{counter}@User.com";
            counter++;
        }

        // Anonymize user
        user.FirstName = "Slettet";
        user.LastName = "Bruger";
        user.Email = updatedEmail;
        user.Country = "-";
        user.Birthday = DateTime.MinValue;

        _userRepository.UpdateUser(user);
        _userRepository.Save();

        return user;
    }
    
    public User PatchUserEmail(PatchUserEmailDto request)
    {
        var user = _userRepository.GetUserOrNull(request.OldEmail);
        if (user == null)
            throw new KeyNotFoundException("Bruger blev ikke fundet.");

        // Ensure email is unique
        if (_userRepository.EmailExists(request.NewEmail))
            throw new ArgumentException("Emailen er allerede i brug.");

        user.Email = request.NewEmail;
        _userRepository.UpdateUser(user);
        _userRepository.Save();

        return user;
    }
    public User PatchUserPassword(string email, PatchUserPasswordDto request)
    {
        var user = _userRepository.GetUserOrNull(email);
        if (user == null)
            throw new KeyNotFoundException("Bruger blev ikke fundet.");

        _securityService.VerifyPasswordOrThrow(request.OldPassword + user.Salt, user.Hash);

        var newSalt = _securityService.GenerateSalt();
        var newHash = _securityService.HashPassword(request.NewPassword + newSalt);

        if (_userRepository.HashExists(newHash))
            throw new ArgumentException("Adgangskoden er allerede i brug.");

        user.Salt = newSalt;
        user.Hash = newHash;
        _userRepository.UpdateUser(user);
        _userRepository.Save();

        return user;
    }


}
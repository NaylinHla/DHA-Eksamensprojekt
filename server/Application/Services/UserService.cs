using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Models.Dtos.RestDtos.Request;
using Core.Domain.Entities;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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
}
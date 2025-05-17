using Application.Models.Dtos.RestDtos.Request;
using Core.Domain.Entities;

namespace Application.Interfaces;

public interface IUserService
{
    Task <User> DeleteUser(DeleteUserDto request);
    Task <User> PatchUserEmail(PatchUserEmailDto request);
    Task <User> PatchUserPassword(string email, PatchUserPasswordDto request);
}
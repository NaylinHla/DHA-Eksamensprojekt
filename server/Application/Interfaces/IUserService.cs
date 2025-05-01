
using Application.Models.Dtos.RestDtos.Request;
using Core.Domain.Entities;

namespace Application.Interfaces;

public interface IUserService
{
    public User DeleteUser(DeleteUserDto request);
    public User PatchUserEmail(PatchUserEmailDto request);
}
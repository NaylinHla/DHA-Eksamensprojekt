using Core.Domain.Entities;

namespace Application.Interfaces.Infrastructure.Postgres;

public interface IUserRepository
{
    User? GetUserOrNull(string email);
    User AddUser(User user);
}
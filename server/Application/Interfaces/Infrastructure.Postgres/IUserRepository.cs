using Core.Domain.Entities;

namespace Application.Interfaces.Infrastructure.Postgres;

public interface IUserRepository
{
    User? GetUserOrNull(string email);
    User AddUser(User user);
    bool EmailExists(string email);
    public bool HashExists(string hash);
    void UpdateUser(User user);
    void Save();
}
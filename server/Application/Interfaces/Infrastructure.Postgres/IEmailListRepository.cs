using Core.Domain.Entities;

namespace Application.Interfaces.Infrastructure.Postgres;

public interface IEmailListRepository
{
    void Add(EmailList emailEntry);
    void RemoveByEmail(string email);
    void Save();
    bool EmailExists(string email);
    List<string> GetAllEmails();

}
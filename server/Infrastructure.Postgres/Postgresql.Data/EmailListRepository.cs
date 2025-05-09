using Application.Interfaces.Infrastructure.Postgres;
using Core.Domain.Entities;
using System.Linq;
using Infrastructure.Postgres.Scaffolding;

namespace Infrastructure.Postgres.Postgresql.Data;

public class EmailListRepository(MyDbContext context) : IEmailListRepository
{
    public void Add(EmailList emailEntry)
    {
        context.EmailList.Add(emailEntry);
    }
    
    public List<string> GetAllEmails()
    {
        return context.EmailList.Select(e => e.Email).ToList();
    }

    public void RemoveByEmail(string email)
    {
        var entry = context.EmailList.FirstOrDefault(e => e.Email == email);
        if (entry != null)
            context.EmailList.Remove(entry);
    }

    public bool EmailExists(string email)
    {
        return context.EmailList.Any(e => e.Email == email);
    }

    public void Save()
    {
        context.SaveChanges();
    }
}
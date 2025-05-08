using Application.Interfaces.Infrastructure.Postgres;
using Core.Domain.Entities;
using System.Linq;
using Infrastructure.Postgres.Scaffolding;

namespace Infrastructure.Postgres.Repositories;

public class EmailListRepository : IEmailListRepository
{
    private readonly MyDbContext _context;

    public EmailListRepository(MyDbContext context)
    {
        _context = context;
    }

    public void Add(EmailList emailEntry)
    {
        _context.EmailList.Add(emailEntry);
    }
    
    public List<string> GetAllEmails()
    {
        return _context.EmailList.Select(e => e.Email).ToList();
    }

    public void RemoveByEmail(string email)
    {
        var entry = _context.EmailList.FirstOrDefault(e => e.Email == email);
        if (entry != null)
            _context.EmailList.Remove(entry);
    }

    public bool EmailExists(string email)
    {
        return _context.EmailList.Any(e => e.Email == email);
    }

    public void Save()
    {
        _context.SaveChanges();
    }
}
using Application.Interfaces.Infrastructure.Postgres;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Postgres.Postgresql.Data;

public class UserSettingsRepository : IUserSettingsRepository
{
    private readonly MyDbContext _context;

    public UserSettingsRepository(MyDbContext context)
    {
        _context = context;
    }

    public void Add(UserSettings settings)
    {
        _context.UserSettings.Add(settings);
        _context.SaveChanges();
    }

    public UserSettings? GetByUserId(Guid userId)
    {
        return _context.UserSettings.FirstOrDefault(s => s.UserId == userId);
    }

    public void Update(UserSettings settings)
    {
        _context.UserSettings.Update(settings);
        _context.SaveChanges();
    }
}
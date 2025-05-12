using Application.Interfaces.Infrastructure.Postgres;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;

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
}

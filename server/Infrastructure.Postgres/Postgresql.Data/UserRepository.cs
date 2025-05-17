using Application.Interfaces.Infrastructure.Postgres;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Postgres.Postgresql.Data;

public class UserRepository(MyDbContext ctx) : IUserRepository
{
    public User? GetUserOrNull(string email) => 
        ctx.Users.FirstOrDefault(u => u.Email == email);

    public bool EmailExists(string email) => 
        ctx.Users.Any(u => u.Email == email);
    
    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) => 
        ctx.Users.AnyAsync(u => u.Email == email, ct);

    public bool HashExists(string hash) => 
        ctx.Users.Any(u => u.Hash == hash);
    
    public void UpdateUser(User user) =>
        ctx.Users.Update(user);

    public void Save() =>
        ctx.SaveChanges();

    public User AddUser(User user)
    {
        ctx.Users.Add(user);
        ctx.SaveChanges();
        return user;
    }
}
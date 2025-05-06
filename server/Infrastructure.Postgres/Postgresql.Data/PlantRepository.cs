using Application.Interfaces.Infrastructure.Postgres;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Postgres.Postgresql.Data;

public class PlantRepository(MyDbContext ctx) : IPlantRepository
{
    public Task<List<Plant>> GetAllPlantsAsync(Guid userId) =>
        ctx.UserPlants
            .AsNoTracking()
            .Where(up => up.UserId == userId)
            .Select  (up => up.Plant!)
            .ToListAsync();

    public Task<Plant?> GetPlantByIdAsync(Guid id) =>
        ctx.Plants.FirstOrDefaultAsync(p => p.PlantId == id);
    
    public async Task<Plant> AddPlantAsync(Guid userId,Plant plant)
    {
        ctx.Plants.Add(plant);
        ctx.UserPlants.Add(new UserPlant { UserId = userId, PlantId = plant.PlantId });
        await ctx.SaveChangesAsync();
        return plant;
    }
    
    public Task SaveChangesAsync() => ctx.SaveChangesAsync();

    public async Task<Plant> MarkPlantAsDeadAsync(Guid id)
    {
        var plant = await GetPlantByIdAsync(id) ?? throw new KeyNotFoundException();
        plant.IsDead = true;
        await ctx.SaveChangesAsync();
        return plant;
    }

    public async Task<Plant> WaterPlantAsync(Guid id)
    {
        var plant = await GetPlantByIdAsync(id) ?? throw new KeyNotFoundException();
        plant.LastWatered = DateTime.UtcNow;
        await ctx.SaveChangesAsync();
        return plant;
    }

    public async Task WaterAllPlantsAsync(Guid userId)
    {
        await ctx.Plants
            .Where(p => p.UserPlants.Any(up => up.UserId == userId))
            .ExecuteUpdateAsync(p => p.SetProperty(
                pl => pl.LastWatered,
                _ => DateTime.UtcNow));
    }
}
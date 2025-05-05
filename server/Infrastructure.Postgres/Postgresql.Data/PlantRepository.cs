using Application.Interfaces.Infrastructure.Postgres;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Postgres.Postgresql.Data;

public class PlantRepository(MyDbContext ctx) : IPlantRepository
{
    public async Task<List<Plant>> GetAllPlantsAsync(Guid userId)
    {
        return await ctx.UserPlants
            .AsNoTracking()
            .Where(u => u.UserId == userId)
            .Select(u => u.Plant!)
            .ToListAsync();
    }

    public async Task<Plant> GetPlantByIdAsync(Guid id)
    {
        var query = ctx.Plants.Where(p => p.PlantId == id);
        return await query.FirstOrDefaultAsync() ?? throw new KeyNotFoundException();
    }
    
    public async Task<Plant> AddPlantAsync(Guid userId,Plant plant)
    {
        ctx.Plants.Add(plant);
        
        ctx.UserPlants.Add(new UserPlant
        {
            UserId  = userId,
            PlantId = plant.PlantId
        });
        
        await ctx.SaveChangesAsync();
        return plant;
    }
    
    public async Task<Plant> EditPlantAsync(Plant plant)
    {
        ctx.Plants.Update(plant);
        await ctx.SaveChangesAsync();
        return plant;
    }

    public async Task<Plant> MarkPlantAsDeadAsync(Guid id)
    {
        var plant = await GetPlantByIdAsync(id);
        plant.IsDead = true;
        ctx.Plants.Update(plant);
        await ctx.SaveChangesAsync();
        return plant;
    }

    public async Task<Plant> WaterPlantAsync(Guid id)
    {
        var plant = await GetPlantByIdAsync(id);
        plant.LastWatered = DateTime.UtcNow;
        ctx.Plants.Update(plant);
        await ctx.SaveChangesAsync();
        return plant;
    }

    public async Task WaterAllPlantsAsync(Guid userId)
    {
        await ctx.Plants
            .Where(p => p.UserPlants.Any(up => up.UserId == userId))
            .ExecuteUpdateAsync(p => p.SetProperty(
                plant => plant.LastWatered,
                _     => DateTime.UtcNow));

        await ctx.SaveChangesAsync();
    }
}
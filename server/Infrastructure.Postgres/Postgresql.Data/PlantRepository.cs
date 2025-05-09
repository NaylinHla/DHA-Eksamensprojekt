using Application.Interfaces.Infrastructure.Postgres;
using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Postgres.Postgresql.Data;

public class PlantRepository(MyDbContext ctx) : IPlantRepository
{
    public Task<List<Plant>> GetAllPlantsAsync(Guid userId)
    {
        return ctx.UserPlants
            .AsNoTracking()
            .Where(up => up.UserId == userId)
            .Select(up => up.Plant!)
            .ToListAsync();
    }

    public Task<Plant?> GetPlantByIdAsync(Guid plantId)
    {
        return ctx.Plants.FirstOrDefaultAsync(p => p.PlantId == plantId);
    }

    public async Task<Plant> AddPlantAsync(Guid userId, Plant plant)
    {
        ctx.Plants.Add(plant);
        ctx.UserPlants.Add(new UserPlant { UserId = userId, PlantId = plant.PlantId });
        await ctx.SaveChangesAsync();
        return plant;
    }

    public async Task<Guid> GetPlantOwnerUserId(Guid plantId)
    {
        var plant = await ctx.UserPlants.FirstOrDefaultAsync(d => d.PlantId == plantId);

        if (plant == null)
            throw new NotFoundException("Plant not found");

        return plant.UserId;
    }

    public async Task DeletePlantAsync(Guid plantId)
    {
        var links = await ctx.UserPlants.Where(up => up.PlantId == plantId)
            .ToListAsync();
        ctx.UserPlants.RemoveRange(links);

        var plant = await GetPlantByIdAsync(plantId) ?? throw new KeyNotFoundException();
        ctx.Plants.Remove(plant);

        await ctx.SaveChangesAsync();
    }

    public Task SaveChangesAsync()
    {
        return ctx.SaveChangesAsync();
    }

    public async Task<Plant> MarkPlantAsDeadAsync(Guid plantId)
    {
        var plant = await GetPlantByIdAsync(plantId) ?? throw new KeyNotFoundException();
        plant.IsDead = true;
        await ctx.SaveChangesAsync();
        return plant;
    }

    public async Task<Plant> WaterPlantAsync(Guid plantId)
    {
        var plant = await GetPlantByIdAsync(plantId) ?? throw new KeyNotFoundException();
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
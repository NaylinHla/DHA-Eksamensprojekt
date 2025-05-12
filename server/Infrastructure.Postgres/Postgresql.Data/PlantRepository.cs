using Application.Interfaces.Infrastructure.Postgres;
using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Infrastructure.Logging;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Postgres.Postgresql.Data;

public class PlantRepository(MyDbContext ctx) : IPlantRepository
{
    public Task<List<Plant>> GetAllPlantsAsync(Guid userId)
    {
        MonitorService.Log.Debug("Entered Get All Plants method in PlantRepository");
        return ctx.UserPlants
            .AsNoTracking()
            .Where(up => up.UserId == userId)
            .Select(up => up.Plant!)
            .ToListAsync();
    }

    public Task<Plant?> GetPlantByIdAsync(Guid plantId)
    {
        MonitorService.Log.Debug("Entered Get Plant by Id method in PlantRepository");
        var plant = ctx.Plants.FirstOrDefaultAsync(p => p.PlantId == plantId);
        if (plant == null)
        {
            MonitorService.Log.Error("Failed to find plant");
            throw new KeyNotFoundException();
        }
        return plant;
    }

    public async Task<Plant> AddPlantAsync(Guid userId, Plant plant)
    {
        MonitorService.Log.Debug("Entered Add Plant Async method in PlantRepository");
        ctx.Plants.Add(plant);
        ctx.UserPlants.Add(new UserPlant { UserId = userId, PlantId = plant.PlantId });
        await ctx.SaveChangesAsync();
        return plant;
    }

    public async Task<Guid> GetPlantOwnerUserId(Guid plantId)
    {
        MonitorService.Log.Debug("Entered Get Plant Owner User Id method in PlantRepository");

        var plant = await ctx.UserPlants.FirstOrDefaultAsync(d => d.PlantId == plantId);

        if (plant != null) return plant.UserId;
        MonitorService.Log.Error("Failed to find plant");
        throw new KeyNotFoundException();
    }

    public async Task DeletePlantAsync(Guid plantId)
    {
        MonitorService.Log.Debug("Entered Delete Plant method in PlantRepository");
        var links = await ctx.UserPlants.Where(up => up.PlantId == plantId)
            .ToListAsync();
        ctx.UserPlants.RemoveRange(links);

        var plant = await GetPlantByIdAsync(plantId) ?? throw new KeyNotFoundException();
        ctx.Plants.Remove(plant);

        await ctx.SaveChangesAsync();
    }

    public Task SaveChangesAsync()
    {
        MonitorService.Log.Debug("Entered Save Changes method in PlantRepository");
        return ctx.SaveChangesAsync();
    }

    public async Task<Plant> MarkPlantAsDeadAsync(Guid plantId)
    {
        MonitorService.Log.Debug("Entered Mark Plant as Dead method in PlantRepository");
        var plant = await GetPlantByIdAsync(plantId) ?? throw new KeyNotFoundException();
        plant.IsDead = true;
        await ctx.SaveChangesAsync();
        return plant;
    }

    public async Task<Plant> WaterPlantAsync(Guid plantId)
    {
        MonitorService.Log.Debug("Entered Water Plant method in PlantRepository");
        var plant = await GetPlantByIdAsync(plantId) ?? throw new KeyNotFoundException();
        plant.LastWatered = DateTime.UtcNow;
        await ctx.SaveChangesAsync();
        return plant;
    }

    public async Task WaterAllPlantsAsync(Guid userId)
    {
        MonitorService.Log.Debug("Entered Water All Plants method in PlantRepository");
        await ctx.Plants
            .Where(p => p.UserPlants.Any(up => up.UserId == userId))
            .ExecuteUpdateAsync(p => p.SetProperty(
                pl => pl.LastWatered,
                _ => DateTime.UtcNow));
    }
}
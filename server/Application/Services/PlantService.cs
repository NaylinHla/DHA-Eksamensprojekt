using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Core.Domain.Entities;

namespace Application.Services;

public class PlantService(IPlantRepository plantRepo) : IPlantService
{
    
    public Task<Plant> GetPlantByIdAsync(Guid plantId)
    {
        return plantRepo.GetPlantByIdAsync(plantId);
    }
    
    public Task<List<Plant>> GetAllPlantsAsync(Guid userId)
    {
        return plantRepo.GetAllPlantsAsync(userId);
    }
    
    public Task<Plant> CreatePlantAsync(Guid userId, Plant plant)
    {
        return plantRepo.AddPlantAsync(userId, plant);
    }
    
    public Task<Plant> EditPlantAsync(Plant plant)
    {
        return plantRepo.EditPlantAsync(plant);
    }
    
    public Task<Plant> MarkPlantAsDeadAsync(Guid plantId)
    {
        return plantRepo.MarkPlantAsDeadAsync(plantId);
    }
    
    public Task<Plant> WaterPlantAsync(Guid plantId)
    {
        return plantRepo.WaterPlantAsync(plantId);
    }

    public Task WaterAllPlantsAsync(Guid userId)
    {
        return plantRepo.WaterAllPlantsAsync(userId);
    }
}
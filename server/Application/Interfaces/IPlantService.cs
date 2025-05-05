using Core.Domain.Entities;

namespace Application.Interfaces;

public interface IPlantService
{
    Task<Plant> GetPlantByIdAsync(Guid plantId);
    Task<List<Plant>> GetAllPlantsAsync(Guid userId);
    Task<Plant> CreatePlantAsync(Guid userId, Plant plant);
    Task<Plant> EditPlantAsync(Plant plant);
    Task<Plant> MarkPlantAsDeadAsync(Guid plantId);
    Task<Plant> WaterPlantAsync(Guid plantId);
    Task WaterAllPlantsAsync(Guid userId);
    
}
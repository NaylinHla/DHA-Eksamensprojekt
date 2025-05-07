using Core.Domain.Entities;

namespace Application.Interfaces.Infrastructure.Postgres;

public interface IPlantRepository
{
    Task<Plant?> GetPlantByIdAsync(Guid plantId);
    Task<List<Plant>> GetAllPlantsAsync(Guid userId);
    
    Task<Plant> AddPlantAsync(Guid userId, Plant plant);
    Task SaveChangesAsync();
    
    Task<Plant> MarkPlantAsDeadAsync(Guid plantId);
    Task<Plant> WaterPlantAsync(Guid plantId);
    Task WaterAllPlantsAsync(Guid userId);
}
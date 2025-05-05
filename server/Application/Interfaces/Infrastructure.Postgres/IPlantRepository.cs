using Core.Domain.Entities;

namespace Application.Interfaces.Infrastructure.Postgres;

public interface IPlantRepository
{
    Task<List<Plant>> GetAllPlantsAsync(Guid userId);
    Task<Plant> GetPlantByIdAsync(Guid plantId);
    Task<Plant> AddPlantAsync(Guid userId, Plant plant);
    Task<Plant> EditPlantAsync(Plant plant);
    Task<Plant> MarkPlantAsDeadAsync(Guid plantId);
    Task<Plant> WaterPlantAsync(Guid plantId);
    Task WaterAllPlantsAsync(Guid userId);
}
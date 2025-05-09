using Application.Models;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;

namespace Application.Interfaces;

public interface IPlantService
{
    Task<Plant?> GetPlantByIdAsync(Guid plantId);
    Task<List<Plant>> GetAllPlantsAsync(Guid userId);

    Task<Plant> CreatePlantAsync(Guid userId, PlantCreateDto dto);
    Task DeletePlantAsync(Guid plantId, JwtClaims claims);
    Task<Plant> EditPlantAsync(Guid plantId, PlantEditDto dto, JwtClaims claims);
    Task<Plant> MarkPlantAsDeadAsync(Guid plantId);
    Task<Plant> WaterPlantAsync(Guid plantId);
    Task WaterAllPlantsAsync(Guid userId);
}
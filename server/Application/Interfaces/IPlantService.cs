using Application.Models;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;

namespace Application.Interfaces;

public interface IPlantService
{
    Task<Plant?> GetPlantByIdAsync(Guid plantId, JwtClaims claims);
    Task<List<Plant>> GetAllPlantsAsync(Guid userId, JwtClaims claims);

    Task<Plant> CreatePlantAsync(Guid userId, PlantCreateDto dto);
    Task DeletePlantAsync(Guid plantId, JwtClaims claims);
    Task<Plant> EditPlantAsync(Guid plantId, PlantEditDto dto, JwtClaims claims);
    Task<Plant> MarkPlantAsDeadAsync(Guid plantId, JwtClaims claims);
    Task<Plant> WaterPlantAsync(Guid plantId, JwtClaims claims);
    Task WaterAllPlantsAsync(Guid userId, JwtClaims claims);
}
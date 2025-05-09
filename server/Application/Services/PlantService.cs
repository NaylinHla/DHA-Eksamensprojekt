using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Models;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;

namespace Application.Services;

public class PlantService(IPlantRepository plantRepo) : IPlantService
{
    
    public Task<Plant?> GetPlantByIdAsync(Guid plantId) => plantRepo.GetPlantByIdAsync(plantId);
    
    public Task<List<Plant>> GetAllPlantsAsync(Guid userId) => plantRepo.GetAllPlantsAsync(userId);
    
    public async Task<Plant> CreatePlantAsync(Guid userId, PlantCreateDto dto)
    {
        var plant = new Plant
        {
            PlantId = Guid.NewGuid(),
            PlantName = dto.PlantName,
            PlantType = dto.PlantType,
            PlantNotes = dto.PlantNotes ?? string.Empty,
            Planted = dto.Planted ?? DateTime.UtcNow,
            WaterEvery = dto.WaterEvery,
            IsDead = false
        };
        
        return await plantRepo.AddPlantAsync(userId, plant);
    }

    public async Task DeletePlantAsync(Guid plantId, JwtClaims claims)
    {
        var plantOwnerId = await plantRepo.GetPlantOwnerUserId(plantId);
        if (plantOwnerId != Guid.Parse(claims.Id))
        {
            throw new UnauthorizedAccessException("This plant does not belong to you");
        }
        var plantToDelete = plantRepo.GetPlantByIdAsync(plantId);
        if (plantToDelete.Result == null)
            throw new KeyNotFoundException("Plant not found.");
        if (!plantToDelete.Result.IsDead)
            throw new ArgumentException("Plant is not dead. Mark it as dead before deleting it.");
        await plantRepo.DeletePlantAsync(plantId);
    }
    
    public async Task<Plant> EditPlantAsync(Guid plantId, PlantEditDto dto, JwtClaims claims)
    {
        var plantOwnerId = await plantRepo.GetPlantOwnerUserId(plantId);
        if (plantOwnerId != Guid.Parse(claims.Id))
        {
            throw new UnauthorizedAccessException("This plant does not belong to you");
        }
        
        var plant = await plantRepo.GetPlantByIdAsync(plantId) ?? throw new KeyNotFoundException();

        if (dto.PlantName   is not null) plant.PlantName   = dto.PlantName;
        if (dto.PlantType   is not null) plant.PlantType   = dto.PlantType;
        if (dto.PlantNotes  is not null) plant.PlantNotes  = dto.PlantNotes;
        if (dto.Planted     is not null) plant.Planted     = dto.Planted;
        if (dto.LastWatered is not null) plant.LastWatered = dto.LastWatered;
        if (dto.WaterEvery  is not null) plant.WaterEvery  = dto.WaterEvery;
        if (dto.IsDead      is not null) plant.IsDead      = dto.IsDead.Value;
        
        await plantRepo.SaveChangesAsync();
        return plant;
    }
    
    public Task<Plant> MarkPlantAsDeadAsync(Guid plantId) => plantRepo.MarkPlantAsDeadAsync(plantId);
    public Task<Plant> WaterPlantAsync(Guid plantId) => plantRepo.WaterPlantAsync(plantId);
    public Task WaterAllPlantsAsync(Guid userId) => plantRepo.WaterAllPlantsAsync(userId);
}
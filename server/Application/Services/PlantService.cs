using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;

namespace Application.Services;

public class PlantService(IPlantRepository plantRepo) : IPlantService
{
    
    public Task<Plant?> GetPlantByIdAsync(Guid id) => plantRepo.GetPlantByIdAsync(id);
    
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
    
    public async Task<Plant> EditPlantAsync(Guid plantId, PlantEditDto dto)
    {
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
    
    public Task<Plant> MarkPlantAsDeadAsync(Guid id) => plantRepo.MarkPlantAsDeadAsync(id);
    public Task<Plant> WaterPlantAsync(Guid id) => plantRepo.WaterPlantAsync(id);
    public Task WaterAllPlantsAsync(Guid userId) => plantRepo.WaterAllPlantsAsync(userId);
}
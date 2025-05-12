using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Models;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using Infrastructure.Logging;

namespace Application.Services;

public class PlantService(IPlantRepository plantRepo) : IPlantService
{
    private const string PlantNotFound = "Plant not found.";

    public async Task<Plant?> GetPlantByIdAsync(Guid plantId, JwtClaims claims)
    {
        MonitorService.Log.Debug("Entered Get Plant By Id Async method in PlantService");
        var plantOwnerId = await plantRepo.GetPlantOwnerUserId(plantId);
        if (plantOwnerId == Guid.Parse(claims.Id)) return await plantRepo.GetPlantByIdAsync(plantId);
        MonitorService.Log.Error("User tried to delete plant that does not belong to them");
        throw new AuthenticationException();

    } 

    public async Task<List<Plant>> GetAllPlantsAsync(Guid userId, JwtClaims claims)
    {
        MonitorService.Log.Debug("Entered Get All Plants Async method in PlantService");
        if (userId == Guid.Parse(claims.Id)) return await plantRepo.GetAllPlantsAsync(userId);
        MonitorService.Log.Error("User tried to get all plants that does not belong to them");
        throw new AuthenticationException();
    }

    public async Task<Plant> CreatePlantAsync(Guid userId, PlantCreateDto dto)
    {
        MonitorService.Log.Debug("Entered Create Plant Async method in PlantService");
        var plant = new Plant
        {
            PlantId = Guid.NewGuid(),
            PlantName = dto.PlantName,
            PlantType = dto.PlantType,
            PlantNotes = dto.PlantNotes,
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
            MonitorService.Log.Error("User tried to delete plant that does not belong to them");
            throw new AuthenticationException();
        }

        var plantToDelete = plantRepo.GetPlantByIdAsync(plantId);
        if (plantToDelete.Result == null)
        {
            MonitorService.Log.Error(PlantNotFound);
            throw new KeyNotFoundException();
        }
        if (!plantToDelete.Result.IsDead)
        {
            MonitorService.Log.Error("User tried to delete plant that is not dead");
            throw new ValidationException();
        }
        await plantRepo.DeletePlantAsync(plantId);
    }

    public async Task<Plant> EditPlantAsync(Guid plantId, PlantEditDto dto, JwtClaims claims)
    {
        MonitorService.Log.Debug("Entered Edit Plant Async method in PlantService");
        var plantOwnerId = await plantRepo.GetPlantOwnerUserId(plantId);
        if (plantOwnerId != Guid.Parse(claims.Id))
        {
            MonitorService.Log.Error("User tried to edit plant that does not belong to them");
            throw new AuthenticationException();
        }

        var plant = await plantRepo.GetPlantByIdAsync(plantId);

        if (plant == null)
        {
            MonitorService.Log.Error(PlantNotFound);
            throw new KeyNotFoundException();
        }
        
        if (dto.PlantName is not null) plant.PlantName = dto.PlantName;
        if (dto.PlantType is not null) plant.PlantType = dto.PlantType;
        if (dto.PlantNotes is not null) plant.PlantNotes = dto.PlantNotes;
        if (dto.Planted is not null) plant.Planted = dto.Planted;
        if (dto.LastWatered is not null) plant.LastWatered = dto.LastWatered;
        if (dto.WaterEvery is not null) plant.WaterEvery = dto.WaterEvery;
        if (dto.IsDead is not null) plant.IsDead = dto.IsDead.Value;

        await plantRepo.SaveChangesAsync();
        return plant;
    }

    public async Task<Plant> MarkPlantAsDeadAsync(Guid plantId, JwtClaims claims)
    {
        MonitorService.Log.Debug("Entered Mark Plant As Dead Async method in PlantService");
        var plant = await plantRepo.GetPlantByIdAsync(plantId);

        if (plant == null)
        {
            MonitorService.Log.Error(PlantNotFound);
            throw new KeyNotFoundException();
        }
        
        var plantOwnerId = await plantRepo.GetPlantOwnerUserId(plantId);
        if (plantOwnerId == Guid.Parse(claims.Id)) return await plantRepo.MarkPlantAsDeadAsync(plantId);
        MonitorService.Log.Error("User tried to mark plant as dead that does not belong to them");
        throw new AuthenticationException();

    }

    public async Task<Plant> WaterPlantAsync(Guid plantId, JwtClaims claims)
    {
        MonitorService.Log.Debug("Entered Water Plant Async method in PlantService");
        var plant = await plantRepo.GetPlantByIdAsync(plantId);
        
        if (plant == null)
        {
            MonitorService.Log.Error(PlantNotFound);
            throw new KeyNotFoundException();
        }
        var plantOwnerId = await plantRepo.GetPlantOwnerUserId(plantId);
        if (plantOwnerId == Guid.Parse(claims.Id)) return await plantRepo.WaterPlantAsync(plantId);
        MonitorService.Log.Error("User tried to water plant that does not belong to them");
        throw new AuthenticationException();

    }
    public async Task WaterAllPlantsAsync(Guid userId, JwtClaims claims)
    {
        if (userId != Guid.Parse(claims.Id))
        {
            MonitorService.Log.Error("User tried to water all plants that does not belong to them");
            throw new AuthenticationException();
        }
        await plantRepo.WaterAllPlantsAsync(userId);
    } 
}
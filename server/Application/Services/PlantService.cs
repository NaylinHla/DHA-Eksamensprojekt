using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
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
            Planted = dto.Planted,
            WaterEvery = dto.WaterEvery,
            IsDead = false
        };

        return await plantRepo.AddPlantAsync(userId, plant);
    }

    public async Task DeletePlantAsync(Guid plantId, JwtClaims claims)
    {
        var plantOwnerId = await plantRepo.GetPlantOwnerUserId(plantId);
        
        var plantToDelete = plantRepo.GetPlantByIdAsync(plantId);

        if (plantToDelete.Result is { IsDead: false })
        {
            MonitorService.Log.Error("User tried to delete plant that is not dead");
            throw new ValidationException();
        }
        if (plantOwnerId != Guid.Parse(claims.Id))
        {
            MonitorService.Log.Error("User tried to delete plant that does not belong to them");
            throw new AuthenticationException();
        }
        await plantRepo.DeletePlantAsync(plantId);
    }

    public async Task<Plant> EditPlantAsync(Guid plantId, PlantEditDto dto, JwtClaims claims)
    {
        MonitorService.Log.Debug("Entered Edit Plant Async method in PlantService");
        var plantOwnerId = await plantRepo.GetPlantOwnerUserId(plantId);
        var plant = await plantRepo.GetPlantByIdAsync(plantId);
        if (plantOwnerId != Guid.Parse(claims.Id))
        {
            MonitorService.Log.Error("User tried to edit plant that does not belong to them");
            throw new AuthenticationException();
        }
        plant.PlantName = dto.PlantName ?? plant.PlantName;
        plant.PlantType = dto.PlantType ?? plant.PlantType;
        plant.PlantNotes = dto.PlantNotes ?? plant.PlantNotes;
        plant.Planted = dto.Planted ?? plant.Planted;
        plant.LastWatered = dto.LastWatered ?? plant.LastWatered;
        plant.WaterEvery = dto.WaterEvery ?? plant.WaterEvery;
        plant.IsDead = dto.IsDead ?? plant.IsDead;

        await plantRepo.SaveChangesAsync();
        return plant;
    }

    public async Task<Plant> MarkPlantAsDeadAsync(Guid plantId, JwtClaims claims)
    {
        MonitorService.Log.Debug("Entered Mark Plant As Dead Async method in PlantService");
        var plant = await plantRepo.GetPlantByIdAsync(plantId);
        
        var plantOwnerId = await plantRepo.GetPlantOwnerUserId(plantId);
        if (plantOwnerId == Guid.Parse(claims.Id)) return await plantRepo.MarkPlantAsDeadAsync(plant.PlantId);
        MonitorService.Log.Error("User tried to mark plant as dead that does not belong to them");
        throw new AuthenticationException();

    }

    public async Task<Plant> WaterPlantAsync(Guid plantId, JwtClaims claims)
    {
        MonitorService.Log.Debug("Entered Water Plant Async method in PlantService");
        var plant = await plantRepo.GetPlantByIdAsync(plantId);

        var plantOwnerId = await plantRepo.GetPlantOwnerUserId(plantId);
        if (plantOwnerId == Guid.Parse(claims.Id)) return await plantRepo.WaterPlantAsync(plant.PlantId);
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
﻿using System.Security.Authentication;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Models;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using FluentValidation;
using Infrastructure.Logging;

namespace Application.Services;

public class PlantService(
    IPlantRepository plantRepo,
    IValidator<PlantCreateDto> plantCreateValidator,
    IValidator<PlantEditDto> plantEditValidator) : IPlantService
{

    private const string PlantNotFound = "No plant with that id was found.";
    
    public async Task<Plant?> GetPlantByIdAsync(Guid plantId, JwtClaims claims)
    {
        MonitorService.Log.Debug("Entered Get Plant By Id Async method in PlantService");
        var plantOwnerId = await plantRepo.GetPlantOwnerUserId(plantId);
        if (plantOwnerId == Guid.Parse(claims.Id)) return await plantRepo.GetPlantByIdAsync(plantId);
        MonitorService.Log.Error("User tried to delete plant that does not belong to them");
        throw new AuthenticationException();

    } 

    public async Task<List<Plant?>> GetAllPlantsAsync(Guid userId, JwtClaims claims)
    {
        MonitorService.Log.Debug("Entered Get All Plants Async method in PlantService");
        if (userId == Guid.Parse(claims.Id)) return await plantRepo.GetAllPlantsAsync(userId);
        MonitorService.Log.Error("User tried to get all plants that does not belong to them");
        throw new AuthenticationException();
    }

    public async Task<Plant> CreatePlantAsync(Guid userId, PlantCreateDto dto)
    {
        MonitorService.Log.Debug("Entered Create Plant Async method in PlantService");
        
        var createResult = await plantCreateValidator.ValidateAsync(dto, CancellationToken.None);
        if (!createResult.IsValid)
            throw new ValidationException(createResult.Errors);
        
        
        var plant = new Plant
        {
            PlantId = Guid.NewGuid(),
            PlantName = dto.PlantName,
            PlantType = dto.PlantType,
            PlantNotes = dto.PlantNotes,
            Planted = dto.Planted,
            WaterEvery = dto.WaterEvery,
            IsDead = dto.IsDead
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
            throw new ValidationException("Plant is not dead. Mark plant as dead first before trying to delete it.");
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
        
        var plant = await plantRepo.GetPlantByIdAsync(plantId) 
                    ?? throw new KeyNotFoundException(PlantNotFound);
        
        var plantOwnerId = await plantRepo.GetPlantOwnerUserId(plantId);
        if (plantOwnerId != Guid.Parse(claims.Id))
        {
            MonitorService.Log.Error("User tried to edit plant that does not belong to them");
            throw new AuthenticationException();
        }
        
        var editResult = await plantEditValidator.ValidateAsync(dto, CancellationToken.None);
        if (!editResult.IsValid)
            throw new ValidationException(editResult.Errors);
        
        plant.PlantName = dto.PlantName ?? plant.PlantName;
        plant.PlantType = dto.PlantType ?? plant.PlantType;
        plant.PlantNotes = dto.PlantNotes ?? plant.PlantNotes;
        plant.LastWatered = dto.LastWatered ?? plant.LastWatered;
        plant.WaterEvery = dto.WaterEvery ?? plant.WaterEvery;

        await plantRepo.SaveChangesAsync();
        return plant;
    }

    public async Task<Plant> MarkPlantAsDeadAsync(Guid plantId, JwtClaims claims)
    {
        MonitorService.Log.Debug("Entered Mark Plant As Dead Async method in PlantService");
        var plant = await plantRepo.GetPlantByIdAsync(plantId) 
                    ?? throw new KeyNotFoundException(PlantNotFound);
        
        var plantOwnerId = await plantRepo.GetPlantOwnerUserId(plantId);
        if (plantOwnerId == Guid.Parse(claims.Id)) return await plantRepo.MarkPlantAsDeadAsync(plant.PlantId);
        MonitorService.Log.Error("User tried to mark plant as dead that does not belong to them");
        throw new AuthenticationException();

    }

    public async Task<Plant> WaterPlantAsync(Guid plantId, JwtClaims claims)
    {
        MonitorService.Log.Debug("Entered Water Plant Async method in PlantService");
        var plant = await plantRepo.GetPlantByIdAsync(plantId) 
                    ?? throw new KeyNotFoundException(PlantNotFound);

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
using Application.Interfaces.Infrastructure.Postgres;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Infrastructure.Logging;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Postgres.Postgresql.Data;

public class AlertConditionRepository(MyDbContext ctx) : IAlertConditionRepository

{
    private const string AlertConditionNotFound = "Alert Condition not found.";
    private const string AlertDupeConditionDevice = "A condition with the same sensor and logic already exists.";

    // ---- ConditionAlertPlant ----

    public async Task<ConditionAlertPlant?> GetConditionAlertPlantIdByConditionAlertIdAsync(Guid conditionAlertPlantId)
    {
        return await ctx.ConditionAlertPlant
            .FirstOrDefaultAsync(d => d.ConditionAlertPlantId == conditionAlertPlantId && !d.IsDeleted);
    }

    public async Task<ConditionAlertPlantResponseDto?> GetConditionAlertPlantByIdAsync(Guid plantId)
    {
        MonitorService.Log.Debug("Entered GetConditionAlertPlantByPlantIdAsync with PlantId: {PlantId}", plantId);

        return await ctx.ConditionAlertPlant
            .Where(p => !p.IsDeleted && p.PlantId == plantId)
            .Select(p => new ConditionAlertPlantResponseDto
            {
                ConditionAlertPlantId = p.ConditionAlertPlantId,
                PlantId = p.PlantId,
                WaterNotify = p.WaterNotify
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<ConditionAlertPlantResponseDto>> GetAllConditionAlertPlantsAsync(Guid userId)
    {
        MonitorService.Log.Debug("Entered GetAllConditionAlertPlantsAsync for UserId: {UserId}", userId);

        return await ctx.ConditionAlertPlant
            .Where(p => !p.IsDeleted && ctx.UserPlants.Any(up => up.PlantId == p.PlantId && up.UserId == userId))
            .Select(p => new ConditionAlertPlantResponseDto
            {
                ConditionAlertPlantId = p.ConditionAlertPlantId,
                PlantId = p.PlantId,
                WaterNotify = p.WaterNotify
            })
            .ToListAsync();
    }

    public async Task<ConditionAlertPlantResponseDto> AddConditionAlertPlantAsync(Guid plantId)
    {
        MonitorService.Log.Debug("Entered AddConditionAlertPlantAsync for PlantId: {PlantId}", plantId);

        // Check if a non-deleted alert already exists for this plant
        var exists = await ctx.ConditionAlertPlant
            .AnyAsync(p => p.PlantId == plantId && !p.IsDeleted);

        if (exists)
        {
            MonitorService.Log.Warning("A notification already exists for PlantId: {PlantId}", plantId);
            throw new InvalidOperationException("A water notification already exists for this plant.");
        }

        var entity = new ConditionAlertPlant
        {
            ConditionAlertPlantId = Guid.NewGuid(),
            PlantId = plantId,
            WaterNotify = true,
            IsDeleted = false
        };

        await ctx.ConditionAlertPlant.AddAsync(entity);
        await SaveChangesAsync();

        return new ConditionAlertPlantResponseDto
        {
            ConditionAlertPlantId = entity.ConditionAlertPlantId,
            PlantId = entity.PlantId,
            WaterNotify = entity.WaterNotify
        };
    }

    public async Task DeleteConditionAlertPlantAsync(Guid conditionAlertPlantId)
    {
        MonitorService.Log.Debug(
            "Entered DeleteConditionAlertPlantAsync with ConditionAlertPlantId: {ConditionAlertPlantId}",
            conditionAlertPlantId);

        var entity = await ctx.ConditionAlertPlant
            .FirstOrDefaultAsync(p => p.ConditionAlertPlantId == conditionAlertPlantId);
        
        if (entity is null || entity.IsDeleted)
        {
            throw new NotFoundException(AlertConditionNotFound);
        }

        // Mark entity as deleted and save changes
        entity.IsDeleted = true;
        await SaveChangesAsync();

        MonitorService.Log.Debug("Deleted Alert Condition Plant with ConditionAlertPlantId: {ConditionAlertPlantId}", conditionAlertPlantId);
    }

    // ---- ConditionAlertUserDevice ----
    public async Task<ConditionAlertUserDevice?> GetConditionAlertUserDeviceIdByConditionAlertIdAsync(Guid conditionAlertUserDeviceId)
    {
        return await ctx.ConditionAlertUserDevice
            .FirstOrDefaultAsync(d => d.ConditionAlertUserDeviceId == conditionAlertUserDeviceId && !d.IsDeleted);
    }
    
    public async Task<List<ConditionAlertUserDeviceResponseDto>> GetConditionsAlertUserDeviceByIdAsync(Guid userDeviceId)
    {
        MonitorService.Log.Debug("Entered GetConditionsAlertUserDeviceByUserDeviceIdAsync with UserDeviceId: {UserDeviceId}", userDeviceId);

        return await ctx.ConditionAlertUserDevice
            .Where(d => !d.IsDeleted && d.UserDeviceId == userDeviceId)
            .Select(d => new ConditionAlertUserDeviceResponseDto
            {
                ConditionAlertUserDeviceId = d.ConditionAlertUserDeviceId,
                UserDeviceId = d.UserDeviceId,
                SensorType = d.SensorType,
                Condition = d.Condition
            })
            .ToListAsync();
    }

    public async Task<List<ConditionAlertUserDeviceResponseDto>> GetAllConditionAlertUserDevicesAsync(Guid userId)
    {
        MonitorService.Log.Debug("Entered GetAllConditionAlertUserDevicesAsync for UserId: {UserId}", userId);

        return await ctx.ConditionAlertUserDevice
            .Include(d => d.UserDevice)
            .Where(d => !d.IsDeleted && d.UserDevice != null && d.UserDevice.UserId == userId)
            .Select(d => new ConditionAlertUserDeviceResponseDto
            {
                ConditionAlertUserDeviceId = d.ConditionAlertUserDeviceId,
                UserDeviceId = d.UserDeviceId,
                SensorType = d.SensorType,
                Condition = d.Condition
            })
            .ToListAsync();
    }

    public async Task<ConditionAlertUserDeviceResponseDto> AddConditionAlertUserDeviceAsync(
        ConditionAlertUserDeviceCreateDto dto)
    {
        MonitorService.Log.Debug("Entered AddConditionAlertUserDeviceAsync for UserDeviceId: {UserDeviceId}", dto.UserDeviceId);

        if (await ConditionAlertExistsAsync(Guid.Parse(dto.UserDeviceId), dto.SensorType, dto.Condition))
        {
            throw new InvalidOperationException(AlertDupeConditionDevice);
        }

        var entity = new ConditionAlertUserDevice
        {
            ConditionAlertUserDeviceId = Guid.NewGuid(),
            UserDeviceId = Guid.Parse(dto.UserDeviceId),
            SensorType = dto.SensorType,
            Condition = dto.Condition,
            IsDeleted = false
        };

        await ctx.ConditionAlertUserDevice.AddAsync(entity);
        await SaveChangesAsync();

        return new ConditionAlertUserDeviceResponseDto
        {
            ConditionAlertUserDeviceId = entity.ConditionAlertUserDeviceId,
            UserDeviceId = entity.UserDeviceId,
            SensorType = entity.SensorType,
            Condition = entity.Condition
        };
    }

    public async Task<ConditionAlertUserDeviceResponseDto> EditConditionAlertUserDeviceAsync(
        ConditionAlertUserDeviceEditDto dto)
    {
        MonitorService.Log.Debug("Entered EditConditionAlertUserDeviceAsync with Id: {Id}",
            dto.ConditionAlertUserDeviceId);

        if (await ConditionAlertExistsAsync(Guid.Parse(dto.UserDeviceId), dto.SensorType, dto.Condition))
        {
            throw new InvalidOperationException(AlertDupeConditionDevice);
        }
        
        var entity = await ctx.ConditionAlertUserDevice
            .FirstOrDefaultAsync(d =>
                d.ConditionAlertUserDeviceId == Guid.Parse(dto.ConditionAlertUserDeviceId) && !d.IsDeleted);

        if (entity is null)
        {
            MonitorService.Log.Error("ConditionAlertUserDevice not found for Id: {Id}", dto.ConditionAlertUserDeviceId);
            throw new NotFoundException(AlertConditionNotFound);
        }

        if (!string.IsNullOrWhiteSpace(dto.SensorType)) entity.SensorType = dto.SensorType;
        if (!string.IsNullOrWhiteSpace(dto.Condition)) entity.Condition = dto.Condition;

        await SaveChangesAsync();

        return new ConditionAlertUserDeviceResponseDto
        {
            ConditionAlertUserDeviceId = entity.ConditionAlertUserDeviceId,
            UserDeviceId = entity.UserDeviceId,
            SensorType = entity.SensorType,
            Condition = entity.Condition
        };
    }
    
    private async Task<bool> ConditionAlertExistsAsync(Guid userDeviceId, string sensorType, string condition, Guid? excludeId = null)
    {
        return await ctx.ConditionAlertUserDevice.AnyAsync(d =>
            d.UserDeviceId == userDeviceId &&
            d.SensorType == sensorType &&
            d.Condition == condition &&
            !d.IsDeleted &&
            (excludeId == null || d.ConditionAlertUserDeviceId != excludeId));
    }

    public async Task DeleteConditionAlertUserDeviceAsync(Guid conditionAlertUserDeviceId)
    {
        MonitorService.Log.Debug("Entered DeleteConditionAlertUserDeviceAsync with Id: {ConditionAlertUserDeviceId}",
            conditionAlertUserDeviceId);

        var entity = await ctx.ConditionAlertUserDevice
            .FirstOrDefaultAsync(d => d.ConditionAlertUserDeviceId == conditionAlertUserDeviceId);

        if (entity is null || entity.IsDeleted)
        {
            throw new NotFoundException(AlertConditionNotFound);
        }

        // Mark entity as deleted and save changes
        entity.IsDeleted = true;
        await SaveChangesAsync();

        MonitorService.Log.Debug("Deleted Alert Condition User Device with Id: {ConditionAlertUserDeviceId}", conditionAlertUserDeviceId);
    }


    // ---- Save Changes ----

    public async Task SaveChangesAsync()
    {
        MonitorService.Log.Debug("Entered SaveChangesAsync in AlertConditionRepository");
        await ctx.SaveChangesAsync();
    }
}
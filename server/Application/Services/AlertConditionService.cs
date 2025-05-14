using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Models;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;

namespace Application.Services;

public class AlertConditionService(IPlantRepository alertConditionRepo) : IAlertConditionService
{
    public Task<ConditionAlertPlant?> GetConditionAlertPlantByIdAsync(Guid conditionAlertPlantId, JwtClaims claims)
    {
        throw new NotImplementedException();
    }

    public Task<List<ConditionAlertPlant>> GetAllConditionAlertPlantsAsync(Guid userId, JwtClaims claims)
    {
        throw new NotImplementedException();
    }

    public Task<ConditionAlertPlant> CreateConditionAlertPlantAsync(Guid conditionAlertPlantId, JwtClaims claims)
    {
        throw new NotImplementedException();
    }

    public Task DeleteConditionAlertPlantAsync(Guid conditionAlertPlantId, JwtClaims claims)
    {
        throw new NotImplementedException();
    }

    public Task<ConditionAlertUserDevice?> GetConditionAlertUserDeviceByIdAsync(Guid conditionAlertUserDeviceId, JwtClaims claims)
    {
        throw new NotImplementedException();
    }

    public Task<List<ConditionAlertUserDevice>> GetAllConditionAlertUserDevicesAsync(Guid userId, JwtClaims claims)
    {
        throw new NotImplementedException();
    }

    public Task<ConditionAlertUserDevice> CreateConditionAlertUserDeviceAsync(ConditionAlertUserDeviceCreateDto dto, JwtClaims claims)
    {
        throw new NotImplementedException();
    }

    public Task<ConditionAlertUserDevice> EditConditionAlertUserDeviceAsync(ConditionAlertUserDeviceEditDto dto, JwtClaims claims)
    {
        throw new NotImplementedException();
    }

    public Task DeleteConditionAlertUserDeviceAsync(Guid conditionAlertUserDeviceId, JwtClaims claims)
    {
        throw new NotImplementedException();
    }
}
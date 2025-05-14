using Application.Models;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;

namespace Application.Interfaces;

public interface IAlertConditionService
{
    Task<ConditionAlertPlant?> GetConditionAlertPlantByIdAsync(Guid conditionAlertPlantId, JwtClaims claims);
    Task<List<ConditionAlertPlant>> GetAllConditionAlertPlantsAsync(Guid userId, JwtClaims claims);
    Task<ConditionAlertPlant> CreateConditionAlertPlantAsync(Guid conditionAlertPlantId, JwtClaims claims);
    Task DeleteConditionAlertPlantAsync(Guid conditionAlertPlantId, JwtClaims claims);

    Task<ConditionAlertUserDevice?> GetConditionAlertUserDeviceByIdAsync(Guid conditionAlertUserDeviceId,
        JwtClaims claims);

    Task<List<ConditionAlertUserDevice>> GetAllConditionAlertUserDevicesAsync(Guid userId, JwtClaims claims);

    Task<ConditionAlertUserDevice> CreateConditionAlertUserDeviceAsync(ConditionAlertUserDeviceCreateDto dto,
        JwtClaims claims);

    Task<ConditionAlertUserDevice> EditConditionAlertUserDeviceAsync(ConditionAlertUserDeviceEditDto dto,
        JwtClaims claims);

    Task DeleteConditionAlertUserDeviceAsync(Guid conditionAlertUserDeviceId, JwtClaims claims);
}
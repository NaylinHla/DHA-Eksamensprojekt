using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;

namespace Application.Interfaces.Infrastructure.Postgres;

public interface IAlertConditionRepository
{
    // ---- ConditionAlertPlant ----

    Task<ConditionAlertPlant?> GetConditionAlertPlantByIdAsync(Guid conditionAlertPlantId);
    Task<List<ConditionAlertPlant>> GetAllConditionAlertPlantsAsync(Guid userId);
    Task<ConditionAlertPlant> AddConditionAlertPlantAsync(Guid plantId);
    Task DeleteConditionAlertPlantAsync(Guid conditionAlertPlantId);

    // ---- ConditionAlertUserDevice ----

    Task<ConditionAlertUserDevice?> GetConditionAlertUserDeviceByIdAsync(Guid conditionAlertUserDeviceId);
    Task<List<ConditionAlertUserDevice>> GetAllConditionAlertUserDevicesAsync(Guid userId);
    Task<ConditionAlertUserDevice> AddConditionAlertUserDeviceAsync(ConditionAlertUserDeviceCreateDto dto);
    Task<ConditionAlertUserDevice> EditConditionAlertUserDeviceAsync(ConditionAlertUserDeviceEditDto dto);
    Task DeleteConditionAlertUserDeviceAsync(Guid conditionAlertUserDeviceId);

    Task SaveChangesAsync();
}
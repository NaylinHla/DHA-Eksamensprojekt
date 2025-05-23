using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;

namespace Application.Interfaces.Infrastructure.Postgres;

public interface IAlertConditionRepository
{
    // ---- ConditionAlertPlant ----
    Task<ConditionAlertPlant?> GetConditionAlertPlantIdByConditionAlertIdAsync(Guid conditionAlertPlantId);
    Task<ConditionAlertPlantResponseDto?> GetConditionAlertPlantByIdAsync(Guid plantId);
    Task<List<ConditionAlertPlantResponseDto>> GetAllConditionAlertPlantForAllUserAsync();
    Task<List<ConditionAlertPlantResponseDto>> GetAllConditionAlertPlantsAsync(Guid userId);
    Task<ConditionAlertPlantResponseDto> AddConditionAlertPlantAsync(Guid plantId);
    Task DeleteConditionAlertPlantAsync(Guid conditionAlertPlantId);

    // ---- ConditionAlertUserDevice ----
    Task<ConditionAlertUserDevice?> GetConditionAlertUserDeviceIdByConditionAlertIdAsync(Guid conditionAlertUserDeviceId);
    Task<List<ConditionAlertUserDeviceResponseDto>> GetConditionsAlertUserDeviceByIdAsync(Guid userDeviceId);
    Task<List<ConditionAlertUserDeviceResponseDto>> GetAllConditionAlertUserDevicesAsync(Guid userId);
    Task<ConditionAlertUserDeviceResponseDto> AddConditionAlertUserDeviceAsync(ConditionAlertUserDeviceCreateDto dto);
    Task<ConditionAlertUserDeviceResponseDto> EditConditionAlertUserDeviceAsync(ConditionAlertUserDeviceEditDto dto);
    Task DeleteConditionAlertUserDeviceAsync(Guid conditionAlertUserDeviceId);

    Task<List<string>> IsAlertUserDeviceConditionMeet(IsAlertUserDeviceConditionMeetDto dto);
    Task SaveChangesAsync();
}
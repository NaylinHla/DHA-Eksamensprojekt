using Application.Models;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;

namespace Application.Interfaces;

public interface IAlertConditionService
{
    Task<ConditionAlertPlantResponseDto?> GetConditionAlertPlantByIdAsync(Guid plantId, JwtClaims claims);
    Task<List<ConditionAlertPlantResponseDto>> GetAllConditionAlertPlantsAsync(Guid userId, JwtClaims claims);
    Task<ConditionAlertPlantResponseDto> CreateConditionAlertPlantAsync(ConditionAlertPlantCreateDto dto, JwtClaims claims);
    Task DeleteConditionAlertPlantAsync(Guid conditionAlertPlantId, JwtClaims claims);

    Task<List<ConditionAlertUserDeviceResponseDto>> GetConditionAlertUserDeviceByIdAsync(Guid userDeviceId,
        JwtClaims claims);

    Task<List<ConditionAlertUserDeviceResponseDto>> GetAllConditionAlertUserDevicesAsync(Guid userId, JwtClaims claims);

    Task<ConditionAlertUserDeviceResponseDto> CreateConditionAlertUserDeviceAsync(ConditionAlertUserDeviceCreateDto dto,
        JwtClaims claims);

    Task<ConditionAlertUserDeviceResponseDto> EditConditionAlertUserDeviceAsync(ConditionAlertUserDeviceEditDto dto,
        JwtClaims claims);

    Task DeleteConditionAlertUserDeviceAsync(Guid conditionAlertUserDeviceId, JwtClaims claims);
}
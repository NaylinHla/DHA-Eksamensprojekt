using Application.Models;
using Application.Models.Dtos.MqttDtos.Response;
using Application.Models.Dtos.MqttSubscriptionDto;
using Application.Models.Dtos.RestDtos;

namespace Application.Interfaces;

public interface IGreenhouseDeviceService
{
    Task<List<GetAllSensorHistoryByDeviceIdDto>>
        GetSensorHistoryByDeviceIdAndBroadcast(Guid deviceId, JwtClaims claims);

    Task AddToDbAndBroadcast(DeviceSensorDataDto? dto);
    Task UpdateDeviceFeed(AdminChangesPreferencesDto dto, JwtClaims claims);
    Task DeleteDataAndBroadcast(JwtClaims jwt);
}
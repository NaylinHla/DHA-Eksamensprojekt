using Application.Models;
using Application.Models.Dtos.MqttDtos.Response;
using Application.Models.Dtos.MqttSubscriptionDto;
using Application.Models.Dtos.RestDtos.SensorHistory;

namespace Application.Interfaces;

public interface IGreenhouseDeviceService
{
    Task<List<GetAllSensorHistoryByDeviceIdDto>>
        GetSensorHistoryByDeviceId(Guid deviceId, DateTime? from, DateTime? to, JwtClaims claims);

    Task<GetRecentSensorDataForAllUserDeviceDto> GetRecentSensorDataForAllUserDevicesAsync(JwtClaims claims);
    Task AddToDbAndBroadcast(DeviceSensorDataDto? dto);
    Task DeleteDataFromSpecificDeviceAndBroadcast(Guid deviceId, JwtClaims claims);
}
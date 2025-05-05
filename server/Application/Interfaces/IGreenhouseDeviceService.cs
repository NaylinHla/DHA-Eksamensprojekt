using System.Security.Claims;
using Application.Models;
using Application.Models.Dtos.MqttDtos.Response;
using Application.Models.Dtos.MqttSubscriptionDto;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.RestDtos.SensorHistory;
using Application.Models.Dtos.RestDtos.UserDevice;

namespace Application.Interfaces;

public interface IGreenhouseDeviceService
{
    Task<List<GetAllSensorHistoryByDeviceIdDto>>
        GetSensorHistoryByDeviceId(Guid deviceId, DateTime? from, DateTime? to, JwtClaims claims);
    Task<GetAllUserDeviceDto> GetAllUserDevice(JwtClaims claims);
    Task<GetRecentSensorDataForAllUserDeviceDto> GetRecentSensorDataForAllUserDevicesAsync(JwtClaims claims);
    Task AddToDbAndBroadcast(DeviceSensorDataDto? dto);
    Task UpdateDeviceFeed(AdminChangesPreferencesDto dto, JwtClaims claims);
    Task DeleteDataFromSpecificDeviceAndBroadcast(Guid deviceId, JwtClaims jwt);
}
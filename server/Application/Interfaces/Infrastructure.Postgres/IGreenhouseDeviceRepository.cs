using Application.Models.Dtos.MqttDtos.Response;
using Application.Models.Dtos.RestDtos.SensorHistory;
using Core.Domain.Entities;

namespace Application.Interfaces.Infrastructure.Postgres;

public interface IGreenhouseDeviceRepository
{
    List<SensorHistory> GetRecentSensorHistory();
    Task<Guid> GetDeviceOwnerUserId(Guid deviceId);
    Task<List<SensorHistoryWithDeviceDto>> GetLatestSensorDataForUserDevicesAsync(Guid userId);

    Task<List<GetAllSensorHistoryByDeviceIdDto>> GetSensorHistoryByDeviceIdAsync(Guid deviceId, DateTime? from = null,
        DateTime? to = null);

    Task<SensorHistory> AddSensorHistory(SensorHistory sensorHistory);
    Task<User> GetUserByDeviceId(Guid deviceId);
    Task DeleteDataFromSpecificDevice(Guid deviceId);
}
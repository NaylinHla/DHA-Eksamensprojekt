using Application.Models.Dtos.MqttDtos.Response;
using Core.Domain.Entities;

namespace Application.Interfaces.Infrastructure.Postgres
{
    public interface IGreenhouseDeviceRepository
    {
        List<SensorHistory> GetRecentSensorHistory();
        Task<Guid> GetDeviceOwnerUserId(Guid deviceId);
        Task<List<GetAllSensorHistoryByDeviceIdDto>> GetSensorHistoryByDeviceIdAsync(Guid deviceId);
        Task<SensorHistory> AddSensorHistory(SensorHistory sensorHistory);
        Task<User> GetUserByDeviceId(Guid deviceId);
        Task DeleteAllSensorHistoryData();
    }
}
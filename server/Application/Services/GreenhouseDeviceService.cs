using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models;
using Application.Models.Dtos;
using Application.Models.Dtos.BroadcastModels;
using Application.Models.Dtos.MqttDtos.Response;
using Application.Models.Dtos.MqttSubscriptionDto;
using Application.Models.Dtos.RestDtos.SensorHistory;
using Core.Domain.Entities;
using Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Services;

public class GreenhouseDeviceService(
    IServiceProvider services,
    IConnectionManager connectionManager)
    : IGreenhouseDeviceService
{
    public async Task AddToDbAndBroadcast(DeviceSensorDataDto? dto)
    {
        MonitorService.Log.Debug("Entered AddToDbAndBroadcast method");

        if (dto == null)
        {
            MonitorService.Log.Warning("AddToDbAndBroadcast called with null dto");
            return;
        }

        MonitorService.Log.Debug($"Processing sensor data for DeviceId: {dto.DeviceId}");

        var sensorHistory = new SensorHistory
        {
            SensorHistoryId = Guid.NewGuid(),
            DeviceId = Guid.Parse(dto.DeviceId),
            Temperature = dto.Temperature,
            Humidity = dto.Humidity,
            AirPressure = dto.AirPressure,
            AirQuality = dto.AirQuality,
            Time = dto.Time
        };

        // Create a new scope so we get a fresh DbContext/repository
        using var scope = services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IGreenhouseDeviceRepository>();

        await repo.AddSensorHistory(sensorHistory);
        MonitorService.Log.Debug($"Sensor data added to DB for DeviceId: {dto.DeviceId}");

        var recentHistory = await repo.GetSensorHistoryByDeviceIdAsync(Guid.Parse(dto.DeviceId));

        var broadcast = new ServerBroadcastsLiveDataToDashboard
        {
            Logs = recentHistory
        };

        await connectionManager.BroadcastToTopic(StringConstants.GreenhouseSensorData + "/" + dto.DeviceId, broadcast);
        MonitorService.Log.Debug($"Broadcasted live data for DeviceId: {dto.DeviceId}");
    }

    public async Task<List<GetAllSensorHistoryByDeviceIdDto>> GetSensorHistoryByDeviceId(
        Guid deviceId,
        DateTime? from,
        DateTime? to,
        JwtClaims claims)
    {
        MonitorService.Log.Debug($"Fetching sensor history for DeviceId: {deviceId} with user: {claims.Id}");

        var repo = services.CreateScope()
            .ServiceProvider
            .GetRequiredService<IGreenhouseDeviceRepository>();

        var deviceOwnerId = await repo.GetDeviceOwnerUserId(deviceId);
        if (deviceOwnerId != Guid.Parse(claims.Id))
        {
            MonitorService.Log.Warning($"Unauthorized access attempt by user {claims.Id} on device {deviceId}");
            throw new UnauthorizedAccessException("You do not own this device.");
        }

        MonitorService.Log.Debug($"Authorized access by user {claims.Id} to device {deviceId}");
        return await repo.GetSensorHistoryByDeviceIdAsync(deviceId, from, to);
    }

    public async Task<GetRecentSensorDataForAllUserDeviceDto> GetRecentSensorDataForAllUserDevicesAsync(
        JwtClaims claims)
    {
        MonitorService.Log.Debug($"Fetching recent sensor data for user: {claims.Id}");

        using var scope = services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IGreenhouseDeviceRepository>();

        var records = await repo.GetLatestSensorDataForUserDevicesAsync(Guid.Parse(claims.Id));

        MonitorService.Log.Debug($"Fetched {records.Count} recent sensor data records for user {claims.Id}");

        return new GetRecentSensorDataForAllUserDeviceDto
        {
            SensorHistoryWithDeviceRecords = records
        };
    }

    public async Task DeleteDataFromSpecificDeviceAndBroadcast(Guid deviceId, JwtClaims claims)
    {
        MonitorService.Log.Debug($"User {claims.Id} requested to delete data for device {deviceId}");

        var repo = services.CreateScope()
            .ServiceProvider
            .GetRequiredService<IGreenhouseDeviceRepository>();

        var deviceOwnerId = await repo.GetDeviceOwnerUserId(deviceId);
        if (deviceOwnerId != Guid.Parse(claims.Id))
        {
            MonitorService.Log.Warning($"Unauthorized delete attempt by user {claims.Id} on device {deviceId}");
            throw new UnauthorizedAccessException("You do not own this device.");
        }

        await repo.DeleteDataFromSpecificDevice(deviceId);
        MonitorService.Log.Debug($"Deleted data for device {deviceId} by user {claims.Id}");

        await connectionManager.BroadcastToTopic(
            StringConstants.Dashboard,
            new AdminHasDeletedData()
        );

        MonitorService.Log.Debug($"Broadcasted AdminHasDeletedData event for device {deviceId}");
    }
}

public class AdminHasDeletedData : ApplicationBaseDto
{
    public override string eventType { get; set; } = nameof(AdminHasDeletedData);
}
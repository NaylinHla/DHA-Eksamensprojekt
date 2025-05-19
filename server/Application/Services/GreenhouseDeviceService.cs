using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models;
using Application.Models.Dtos;
using Application.Models.Dtos.BroadcastModels;
using Application.Models.Dtos.MqttDtos.Response;
using Application.Models.Dtos.MqttSubscriptionDto;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.RestDtos.SensorHistory;
using Core.Domain.Entities;
using FluentValidation;
using Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Services;

public class GreenhouseDeviceService(
    IServiceProvider services,
    IConnectionManager connectionManager,
    IValidator<DeviceSensorDataDto> sensorValidator)
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
        
        await sensorValidator.ValidateAndThrowAsync(dto);

        // Save sensor data to DB
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

        //Check and trigger user device condition alerts
        var alertCheckDto = new IsAlertUserDeviceConditionMeetDto
        {
            UserDeviceId = dto.DeviceId,
            Temperature = dto.Temperature,
            Humidity = dto.Humidity,
            AirPressure = dto.AirPressure,
            AirQuality = dto.AirQuality,
            Time = dto.Time
        };
        
        // Single DI scope for repo + alert service
        using var scope = services.CreateScope();
        var repo         = scope.ServiceProvider.GetRequiredService<IGreenhouseDeviceRepository>();
        var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();
        
        
        await alertService.TriggerUserDeviceConditionAsync(alertCheckDto);
        MonitorService.Log.Debug("Triggering alert check for DeviceId: {DeviceId}", dto.DeviceId);
        
        await repo.AddSensorHistory(sensorHistory);
        MonitorService.Log.Debug("Sensor data added to DB for DeviceId: {DeviceId}", dto.DeviceId);
        
        //Broadcast to live dashboard
        var recentHistory = await repo.GetSensorHistoryByDeviceIdAsync(Guid.Parse(dto.DeviceId));

        var broadcast = new ServerBroadcastsLiveDataToDashboard
        {
            Logs = recentHistory
        };

        await connectionManager.BroadcastToTopic(StringConstants.GreenhouseSensorData + "/" + dto.DeviceId, broadcast);
        MonitorService.Log.Debug("Broadcasted live data for DeviceId: {DeviceId}", dto.DeviceId);
    }

    public async Task<List<GetAllSensorHistoryByDeviceIdDto>> GetSensorHistoryByDeviceId(
        Guid deviceId,
        DateTime? from,
        DateTime? to,
        JwtClaims claims)
    {
        MonitorService.Log.Debug("Fetching sensor history for DeviceId: {DeviceId} with user: {UserId}", deviceId, claims.Id);

        var repo = services.CreateScope()
            .ServiceProvider
            .GetRequiredService<IGreenhouseDeviceRepository>();

        var deviceOwnerId = await repo.GetDeviceOwnerUserId(deviceId);
        if (deviceOwnerId != Guid.Parse(claims.Id))
        {
            MonitorService.Log.Warning("Unauthorized access attempt by user {UserId} on device {DeviceId}", claims.Id, deviceId);
            throw new UnauthorizedAccessException("You do not own this device.");
        }

        MonitorService.Log.Debug("Authorized access by user {UserId} to device {DeviceId}", claims.Id, deviceId);
        return await repo.GetSensorHistoryByDeviceIdAsync(deviceId, from, to);
    }

    public async Task<GetRecentSensorDataForAllUserDeviceDto> GetRecentSensorDataForAllUserDevicesAsync(
        JwtClaims claims)
    {
        MonitorService.Log.Debug("Fetching recent sensor data for user: {UserId}", claims.Id);

        using var scope = services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IGreenhouseDeviceRepository>();

        var records = await repo.GetLatestSensorDataForUserDevicesAsync(Guid.Parse(claims.Id));

        MonitorService.Log.Debug("Fetched {Count} recent sensor data records for user {UserId}", records.Count, claims.Id);

        return new GetRecentSensorDataForAllUserDeviceDto
        {
            SensorHistoryWithDeviceRecords = records
        };
    }

    public async Task DeleteDataFromSpecificDeviceAndBroadcast(Guid deviceId, JwtClaims claims)
    {
        MonitorService.Log.Debug("User {UserId} requested to delete data for device {DeviceId}", claims.Id, deviceId);

        var repo = services.CreateScope()
            .ServiceProvider
            .GetRequiredService<IGreenhouseDeviceRepository>();

        var deviceOwnerId = await repo.GetDeviceOwnerUserId(deviceId);
        if (deviceOwnerId != Guid.Parse(claims.Id))
        {
            MonitorService.Log.Warning("Unauthorized delete attempt by user {UserId} on device {DeviceId}", claims.Id, deviceId);
            throw new UnauthorizedAccessException("You do not own this device.");
        }

        await repo.DeleteDataFromSpecificDevice(deviceId);
        MonitorService.Log.Debug("Deleted data for device {DeviceId} by user {UserId}", deviceId, claims.Id);

        await connectionManager.BroadcastToTopic(
            StringConstants.Dashboard,
            new AdminHasDeletedData()
        );

        MonitorService.Log.Debug("Broadcasted AdminHasDeletedData event for device {DeviceId}", deviceId);
    }
}

public class AdminHasDeletedData : ApplicationBaseDto
{
    public override string eventType { get; set; } = nameof(AdminHasDeletedData);
}